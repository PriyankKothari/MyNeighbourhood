using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Datacom.IRIS.Common;
using Datacom.IRIS.DomainModel.DTO.PropertyData;
using Datacom.IRIS.DomainModel.Domain.Constants;
using Datacom.IRIS.DomainModel.Domain.Constants.PropertyData;

namespace Datacom.IRIS.DataAccess.PropertyData
{
    /// <summary>Reads an EDE+ formatted DVR file stream and triggers the data upload to the IRIS SQLServer DB.</summary>
    public class DVRFileReader
    {
        private const char Delimiter = '|';
        private const int RecordTypePosition = 7;
        private const int TrailerRecordRecordCountPosition = 8;
        private const int TrailerRecordAssessmentCountPosition = 9;
        private const string ValidRecordTypes = "ABCDEFGHIJKLMZ0";
        private const int DefaultPreviousRecordTypeAsciiValue = 0;
        private const int MaximumNumberOfARecordTypesPerVRN = 1;
        private const int MaximumNumberOfDRecordTypesPerVRN = 1;

        /// <summary>Values for the current EDE+ record being read</summary>
        private ReadCursor Cursor { get; set; }
        /// <summary>Read cursor used to keep values for the current EDE+ record</summary>
        private class ReadCursor
        {
            public ReadCursor()
            {
                Counts = new Dictionary<string, int>();
                RowNumber = 0;
                RecordType = string.Empty;
                ValuationReferenceNumber = string.Empty;
                vrns = new Dictionary<string, int>();
            }

            public Dictionary<string, int> vrns { get; private set; }
            public string Line { get; set; }
            public string RecordType { get; private set; }
            public Dictionary<string, int> Counts { get; private set; }
            public bool IsValidRecordType { get; private set; }
            public bool IsVRNChange { get; private set; }
            public int RowNumber { get; private set; }
            public int PreviousRecordTypeAsciiValue { get; private set; }
            public string[] PropertyData { get; private set; }
            public string RecordTypeValue { get; private set; }
            public int ARecordCountForVRN { get; set; }
            public int DRecordCountForVRN { get; set; }
            public string PreviousValuationReferenceNumber { get; private set; }
            public int AssessmentCount { get; set; }
            public string ValuationReferenceNumber { get; private set; }
            public int RecordCount
            {
                get
                {
                    //Exclude trailer records from the actual count as the Trailer record (Z Type Record) record count does not include itself.
                    var trailerRecordCount = Counts.ContainsKey(DVRRecordType.Trailer) ? Counts[DVRRecordType.Trailer] : 0;
                    return RowNumber - trailerRecordCount;
                }
            }

            public void Next()
            {
                ++RowNumber;
                PropertyData = Line.Split(Delimiter);
                SetRecordType();                
                SetValuationReferenceNumber();
                UpdateCurrentRecordCounts();
            }
            public void UpdateTrailerRecordCounts(Int64 propertyDataDVRUploadID, ICollection<PropertyDataImportError> importErrors)
            {
                var recordCountString = PropertyData.Length > TrailerRecordRecordCountPosition ? PropertyData[TrailerRecordRecordCountPosition] : "0";
                int recordCount;
                if (!int.TryParse(recordCountString, out recordCount))
                {
                    var errMessage = string.Format(DVRImportErrorMessages.FieldIsRequired, "Trailer record count");
                    importErrors.Add(new PropertyDataImportError(propertyDataDVRUploadID, RowNumber, errMessage, SecurityHelper.CurrentUserName));
                }

                UpdateTrailerRecordCount(recordCount, DVRRecordType.TrailerRecordRecordCount);


                var assessmentCountString = PropertyData.Length > TrailerRecordAssessmentCountPosition ? PropertyData[TrailerRecordAssessmentCountPosition] : "0";
                int assessmentCount;
                if (!int.TryParse(assessmentCountString, out assessmentCount))
                {
                    var errMessage = string.Format(DVRImportErrorMessages.FieldIsRequired, "Trailer assessment count");
                    importErrors.Add(new PropertyDataImportError(propertyDataDVRUploadID, RowNumber, errMessage, SecurityHelper.CurrentUserName));
                }

                UpdateTrailerRecordCount(assessmentCount, DVRRecordType.TrailerRecordAssessmentCount);
            }
            public void SetPreviousValuationReferenceNumber()
            {
                if (!IsVRNChange) return;
                PreviousValuationReferenceNumber = ValuationReferenceNumber;
            }
            private void UpdateCurrentRecordCounts()
            {
                if (Counts.ContainsKey(RecordType))
                {
                    Counts[RecordType]++;
                }
                else
                {
                    Counts.Add(RecordType, 1);
                }
            }
            private void SetValuationReferenceNumber()
            {
                var pos = PropertyDataFieldPositions.CommonFields.RollNumber;
                var rollNumber = PropertyData.Length >= pos ? PropertyData[pos] : "";
                pos = PropertyDataFieldPositions.CommonFields.AssessmentNumber;
                var assessmentNumber = PropertyData.Length >= pos ? PropertyData[pos] : "";
                pos = PropertyDataFieldPositions.CommonFields.AssessmentSuffix;
                var assessmentSuffix = PropertyData.Length >= pos ? PropertyData[pos] : "";

                ValuationReferenceNumber = rollNumber.PadLeft(5, '0') + assessmentNumber.PadLeft(5, '0') + assessmentSuffix;

                ProcessValuationReferenceNumberChanges();
            }
            private void ProcessValuationReferenceNumberChanges()
            {
                IsVRNChange = ValuationReferenceNumber != PreviousValuationReferenceNumber;
                if (!IsVRNChange) return;

                AssessmentCount++;
                PreviousRecordTypeAsciiValue = DefaultPreviousRecordTypeAsciiValue;                
            }
            private void SetRecordType()
            {
                SetPreviousRecordType();

                RecordType = PropertyData.Length >= RecordTypePosition ? PropertyData[RecordTypePosition] : string.Empty;
                RecordTypeValue = RecordType;

                IsValidRecordType = ValidRecordTypes.Contains(RecordType);

                if (RecordType.Trim() == string.Empty)
                    RecordType = DVRRecordType.Missing;
                else if (!IsValidRecordType)
                    RecordType = DVRRecordType.Other;
            }
            private void SetPreviousRecordType()
            {
                PreviousRecordTypeAsciiValue = string.IsNullOrEmpty(RecordType)
                                                   ? DefaultPreviousRecordTypeAsciiValue
                                                   : RecordType[0];
            }
            private void UpdateTrailerRecordCount(int recordCount, string key)
            {
                if (Counts.ContainsKey(key))
                {
                    Counts[key] = recordCount;
                }
                else
                {
                    Counts.Add(key, recordCount);
                }
            }

            public void CheckForDuplicates(List<PropertyDataImportError> importErrors, long propertyDataDVRUploadID)
            {
                var key = ValuationReferenceNumber + RecordType;
                if (vrns.ContainsKey(key))
                {
                    vrns[key]++;
                    var errMessage = string.Format(DVRImportErrorMessages.DuplicateRecordType, RecordType,ValuationReferenceNumber,vrns[key]);
                    importErrors.Add(new PropertyDataImportError(propertyDataDVRUploadID, RowNumber, errMessage, SecurityHelper.CurrentUserName));
                }
                else
                {
                    vrns.Add(key,1);
                }
            }
        }

        private string _userName;
        private string UserName { get { return _userName ?? (_userName = SecurityHelper.CurrentUserName); } }

        public Int64 PropertyDataDVRUploadID { get; set; }        
        public TimeSpan Duration;
        public List<PropertyDataPropertyDetails> PropertyDetails { get; set; }
        public List<PropertyDataRatePayerDetails> RatePayerDetails { get; set; }
        public List<PropertyDataOwnerDetails> OwnerDetails { get; set; }
        public List<PropertyDataPropertyDetailsPart2> PropertyDetailsPart2 { get; set; }
        public List<PropertyDataLegalDescription> LegalDescription { get; set; }
        public List<PropertyDataImportError> ImportErrors { get; set; }
        public List<PropertyDataRecordCounts> RecordCounts { get; set; }

        /// <summary>When the ThrottleCount reaches this limit the processed records are bulk copied to the data store.</summary>
        public int ThrottleUpdateLimit { get; set; }
        /// <summary>Number of records waiting to be copied to data store. Note only the record types we process (Types A-E) increase this count.</summary>
        public int ThrottleCount { get; set; }
        /// <summary>Uploads records to the data store.</summary>
        private readonly IBulkUploader _uploader;

        private readonly TextReader _reader;

        /// <summary>DVR EDE+ File reader</summary>
        public DVRFileReader(TextReader streamReader,long propertyDataDVRUploadID, IBulkUploader uploader, int updateThrottle = 0)
        {
            _uploader = uploader;
            _reader = streamReader;

            PropertyDataDVRUploadID = propertyDataDVRUploadID;

            if (uploader is DVRFileSQLBulkUploader)
            {
                (uploader as DVRFileSQLBulkUploader).DVRFileReader = this;
            }
            ThrottleUpdateLimit = updateThrottle;
        }
        /// <summary>Read EDE+ file and write records to the IRIS DB, recording errors, validation failures and execution times as we go.</summary>
        public void Read()
        {
            InitaliseRead();
            try
            {
                Cursor = new ReadCursor();
                while ((Cursor.Line = _reader.ReadLine()) != null)
                {
                    ValidateLineHasEDEDelimters(); //Throws an error when invalid file type to stop processing, so needs to come first.

                    Cursor.Next();

                    //ValidateRecordTypeSequence();
                    ValidateMandatoryRecordTypesWhenVRNChanges();

                    switch (Cursor.RecordType)
                    {
                        case DVRRecordType.PropertyDetails:
                            if (ValidateExpectFieldsForRecordType(28))
                            {
                                PropertyDetails.Add(new PropertyDataPropertyDetails(ImportErrors, PropertyDataDVRUploadID, Cursor.RowNumber, Cursor.PropertyData, UserName));
                                ThrottleCount++;
                            }
                            ValidateARecordCount();
                            break;
                        case DVRRecordType.RatePayerDetails:
                            if (ValidateExpectFieldsForRecordType(21))
                            {
                                RatePayerDetails.Add(new PropertyDataRatePayerDetails(ImportErrors, PropertyDataDVRUploadID, Cursor.RowNumber, Cursor.PropertyData, UserName));
                                ThrottleCount++;
                            }
                            break;
                        case DVRRecordType.OwnerDetails:
                            if (ValidateExpectFieldsForRecordType(21))
                            {
                                OwnerDetails.Add(new PropertyDataOwnerDetails(ImportErrors, PropertyDataDVRUploadID, Cursor.RowNumber, Cursor.PropertyData, UserName));
                                ThrottleCount++;
                            }
                            break;
                        case DVRRecordType.PropertyDetailsPart2:
                            if (ValidateExpectFieldsForRecordType(94))
                            {
                                PropertyDetailsPart2.Add(new PropertyDataPropertyDetailsPart2(ImportErrors, PropertyDataDVRUploadID, Cursor.RowNumber, Cursor.PropertyData, UserName));
                                ThrottleCount++;
                            }
                            ValidateDRecordCount();
                            break;
                        case DVRRecordType.LegalDescription:
                            if (ValidateExpectFieldsForRecordType(14))
                            {
                                LegalDescription.Add(new PropertyDataLegalDescription(ImportErrors, PropertyDataDVRUploadID, Cursor.RowNumber, Cursor.PropertyData, UserName));
                                ThrottleCount++;
                            }
                            break;
                        case DVRRecordType.Trailer:
                            Cursor.UpdateTrailerRecordCounts(PropertyDataDVRUploadID,ImportErrors);
                            break;
                        case DVRRecordType.SRADetails:
                        case DVRRecordType.BuildingConsentDetails:
                        case DVRRecordType.Objection:
                        case DVRRecordType.SaleDetails:
                        case DVRRecordType.SaleAssessment:
                        case DVRRecordType.SubdivisionDetails:
                        case DVRRecordType.SubdivisionReason:
                        case DVRRecordType.GEMS:
                        case DVRRecordType.Header:
                            break; //We do not process these record types so ignore them.
                        default:
                            var invalidRecordTypeErrorMessage = string.IsNullOrEmpty(Cursor.RecordTypeValue.Trim()) ? string.Format(DVRImportErrorMessages.FieldIsRequired, "Record type") : string.Format(DVRImportErrorMessages.InvalidRecordType, Cursor.RecordTypeValue);
                            ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, invalidRecordTypeErrorMessage, UserName));
                            break;
                    }
                    Cursor.SetPreviousValuationReferenceNumber();
                    BatchUpdatesBasedOnThrottleSetting(_uploader);
                }
                ValidateAAndDRecordExistsForVRN();
                ProcessTrailerRecord();                
                RecordCounts.Add(new PropertyDataRecordCounts(PropertyDataDVRUploadID, Cursor.Counts, UserName));
            }
            catch (Exception ex)
            {
                var exceptionMsgs = string.Format("Msg: {0}, InnerException: {1}", ex.Message, ex.InnerException == null ? "No inner exception." : ex.InnerException.Message);
                var errMessage = string.Format(DVRImportErrorMessages.FileReadError, exceptionMsgs);
                ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, errMessage, UserName));
            }
            finally
            {
                _reader.Close();
                _uploader.BulkUpload();
            }
        }

        private void ProcessTrailerRecord()
        {            
            var adjustCountsAndErrorsIfLastRecordIsTrailerRecord = Cursor.RecordType == DVRRecordType.Trailer;
            if (adjustCountsAndErrorsIfLastRecordIsTrailerRecord)
            {
                //Not required now that we do not record sequence errors
                //
                //Remove out sequence error for trailer record.
                //var expectedTrailerRecordError = string.Format(DVRImportErrorMessages.InvalidFileFormatRecordTypeSequence, DVRRecordType.Trailer, "").Replace(".","").Trim();
                //var lastError = ImportErrors.Last();
                //var isLastErrorTrailerRecordError = lastError.ErrorMessage.Contains(expectedTrailerRecordError);

                //if (isLastErrorTrailerRecordError)
                //    ImportErrors.Remove(lastError);
                
                //Adjust assessment count.
                Cursor.AssessmentCount--;
            }
            ValidateTrailerRecord();
        }

        /// <summary>Reset values for the read</summary>
        private void InitaliseRead()
        {
            PropertyDetails = new List<PropertyDataPropertyDetails>();
            RatePayerDetails = new List<PropertyDataRatePayerDetails>();
            OwnerDetails = new List<PropertyDataOwnerDetails>();
            PropertyDetailsPart2 = new List<PropertyDataPropertyDetailsPart2>();
            LegalDescription = new List<PropertyDataLegalDescription>();
            ImportErrors = new List<PropertyDataImportError>();
            RecordCounts = new List<PropertyDataRecordCounts>();
            ThrottleCount = 0;
        }

        /// <summary>Guards against wrong file formats e.g. binary by raising an error early in the read process.</summary>
        private void ValidateLineHasEDEDelimters()
        {
            var validEDEFileFormat = Cursor.Line.Contains("|");
            if (!validEDEFileFormat)
            {
                ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, DVRImportErrorMessages.InvalidFileFormat, UserName));
                throw new Exception(string.Format("Invalid EDE+ File format the row does not contain delimiters - Row (No: {0}, data: '{1}'). ", Cursor.RowNumber, Cursor.Line));
            }
        }
        /// <summary>Ensures record types are in sequence e.g. A, B, C, D ...</summary>
        private void ValidateRecordTypeSequence()
        {
            if (! Cursor.IsValidRecordType) return;  //Only check sequence for valid record types, as invalid types will be reported as invalid not out of sequence.

            // For a given ValuationReferenceNumber we expect all record types to be in sequence e.g.
            //
            // VRN              RecordType      Required
            //-------------     ----------      -----------------
            // 0001101000A               A      Mandatory per VRN
            // 0001101000A               B      Optional per VRN
            // 0001101000A               C      Optional per VRN
            // 0001101000A               D      Mandatory per VRN
            // 0001101000A               E      Optional per VRN
            // 0001101000A      FGHIJKLMZ0      Ignored
            //
            // We use a simple ASCII value comparison to validate the sequence.
            int recordTypeAsciiValue = Cursor.RecordType[0];
            var isInSequence = recordTypeAsciiValue >= Cursor.PreviousRecordTypeAsciiValue;

            // Ensure first record for VRN is record type A.
            var isFirstRecordForVRN = Cursor.IsVRNChange;
            var isARecord = Cursor.RecordType == DVRRecordType.PropertyDetails;

            var isValidSequence = isFirstRecordForVRN ? isInSequence && isARecord : isInSequence;
            if (isValidSequence) return;
            
            var errorMsg = string.Format(DVRImportErrorMessages.InvalidFileFormatRecordTypeSequence, Cursor.RecordType, Cursor.ValuationReferenceNumber);
            ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, errorMsg, UserName));
        }
        /// <summary>Ensures there is one ARecord and one DRecord for each VRN.</summary>
        private void ValidateMandatoryRecordTypesWhenVRNChanges()
        {
            var isFirstRecord = Cursor.RowNumber <= 1;
            if (isFirstRecord || !Cursor.IsVRNChange || Cursor.RecordType == DVRRecordType.Trailer) return;

            ValidateAAndDRecordExistsForVRN();

            //Reset
            Cursor.ARecordCountForVRN = 0;
            Cursor.DRecordCountForVRN = 0;
        }
        /// <summary>Write error if A or D record is missing</summary>
        private void ValidateAAndDRecordExistsForVRN()
        {
            if (Cursor.ARecordCountForVRN == 0)
            {
                var errorMsg = string.Format(DVRImportErrorMessages.InvalidFileFormatMissingMandatoryRecordTypeForVRN,
                                             DVRRecordType.PropertyDetails, Cursor.PreviousValuationReferenceNumber);
                ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber - 1, errorMsg, UserName));
            }

            if (Cursor.DRecordCountForVRN == 0)
            {
                var errorMsg = string.Format(DVRImportErrorMessages.InvalidFileFormatMissingMandatoryRecordTypeForVRN,
                                             DVRRecordType.PropertyDetailsPart2, Cursor.PreviousValuationReferenceNumber);
                ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber - 1, errorMsg, UserName));
            }
        }
        /// <summary>Ensures there is one ZRecord at the end of the file that contains a record and assessment (VRN) count that matches the actual file counts.</summary>
        private void ValidateTrailerRecord()
        {
            var trailerRecordCount = Cursor.Counts.ContainsKey(DVRRecordType.Trailer) ? Cursor.Counts[DVRRecordType.Trailer] : 0;

            if (Cursor.RecordType == DVRRecordType.Trailer)
            {
                if (Cursor.Counts.ContainsKey(DVRRecordType.TrailerRecordRecordCount) && Cursor.Counts[DVRRecordType.TrailerRecordRecordCount] != Cursor.RecordCount)
                {
                    var errorMsg = string.Format(DVRImportErrorMessages.TrailerRecordInvalidRecordCount, Cursor.Counts[DVRRecordType.TrailerRecordRecordCount], Cursor.RecordCount);
                    ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, errorMsg, UserName));
                }
                if (Cursor.Counts.ContainsKey(DVRRecordType.TrailerRecordAssessmentCount) && Cursor.Counts[DVRRecordType.TrailerRecordAssessmentCount] != Cursor.AssessmentCount)
                {
                    var errorMsg = string.Format(DVRImportErrorMessages.TrailerRecordInvalidAssessmentCount, Cursor.Counts[DVRRecordType.TrailerRecordAssessmentCount], Cursor.AssessmentCount);
                    ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, errorMsg, UserName));
                }
            }
            else
            {
                ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, DVRImportErrorMessages.TrailerRecordIsNotLastRecord, UserName));
            }
            
            if (trailerRecordCount == 0)
                ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, DVRImportErrorMessages.TrailerRecordMissing, UserName));
            else if (trailerRecordCount > 1)
                ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, DVRImportErrorMessages.TrailerRecordsMultipleRecordsFound, UserName));
        }
        /// <summary>Ensures there each record type has the expected number of fields.</summary>
        private bool ValidateExpectFieldsForRecordType(int expectedNoOfFields)
        {
            var noOfFields = Cursor.PropertyData.Length;
            if (noOfFields != expectedNoOfFields)
            {
                var errorMsg = string.Format(DVRImportErrorMessages.InvalidFileFormatUnexpectedNoOfFields,expectedNoOfFields,Cursor.RecordType,noOfFields);
                ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, errorMsg, UserName));
                return false;
            }
            return true;
        }
        /// <summary>Enforces maximum number of Property details records (Type A) per VRN.</summary>
        private void ValidateARecordCount()
        {
            Cursor.ARecordCountForVRN++;

            if (Cursor.ARecordCountForVRN <= MaximumNumberOfARecordTypesPerVRN)
            {
                Cursor.CheckForDuplicates(ImportErrors, PropertyDataDVRUploadID);
            }
            else
            {
                var errorMsg = string.Format(DVRImportErrorMessages.InvalidFileFormatTooManyMandatoryRecordTypesForVRN,
                                             DVRRecordType.PropertyDetails, Cursor.PreviousValuationReferenceNumber);
                ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, errorMsg,
                                                             UserName));
            }
        }
        /// <summary>Enforces maximum number of Property details (2) records (Type D) per VRN.</summary>
        private void ValidateDRecordCount()
        {
            Cursor.DRecordCountForVRN++;

            if (Cursor.DRecordCountForVRN <= MaximumNumberOfDRecordTypesPerVRN)
            {
                Cursor.CheckForDuplicates(ImportErrors, PropertyDataDVRUploadID);
            }
            else
            {
                var errorMsg = string.Format(DVRImportErrorMessages.InvalidFileFormatTooManyMandatoryRecordTypesForVRN,
                                             DVRRecordType.PropertyDetailsPart2, Cursor.PreviousValuationReferenceNumber);
                ImportErrors.Add(new PropertyDataImportError(PropertyDataDVRUploadID, Cursor.RowNumber, errorMsg,
                                                             UserName));
            }
        }

        /// <summary>Bulk copy DVR file records to SQL when the Throttle limit is reached.</summary>
        private void BatchUpdatesBasedOnThrottleSetting(IBulkUploader uploader)
        {
            if (ThrottleUpdateLimit == 0 || ThrottleCount < ThrottleUpdateLimit) return;

            uploader.BulkUpload();
            InitaliseRead();
            ThrottleCount = 0;
        }        
    }
}