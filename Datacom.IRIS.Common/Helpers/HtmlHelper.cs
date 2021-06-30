using System.Collections.Generic;
using System.Text;

namespace Datacom.IRIS.Common.Helpers
{
    public class HtmlHelper
    {
        private class TagItem
        {
            public TagItem(string tag, int startPosition, int? endPosition)
            {
                TagTypeName = tag;
                StartPositionInHtml = startPosition;
                EndPositionInHtml = endPosition;
            }

            public string TagTypeName { get; set; } // e.g. <b> would be b and <table> would be table
            public int StartPositionInHtml { get; set; } // 0 based tag position in html.   
            public int? EndPositionInHtml { get; set; } // 0 based tag position in html.   
        }

        /// <summary>Left truncates a html string returning the required content length (content is all text outside of the html tags) 
        /// and tidies up tags that are left open by the truncation. It also treats Ampersand codes as single characters.</summary>
        /// <param name="html">html to truncate, not needs to valid, does not deal with out of order tags</param>
        /// <param name="contentLength">length of the content to return.</param>
        /// <returns>Truncated html string</returns>
        public static string TruncateLeft(string html, int contentLength)
        {
            if (contentLength == 0) return "";
            if (contentLength >= html.Length || contentLength < 0) return html; //No truncation required.

            var truncatedHtml = "";
            var tags = new Stack<TagItem>(); //Add all open tags to stack then remove them if they are not closed.
            var contentCount = 0;
            var isHtmlTag = false;
            const int notAppersandCode = -1;
            var AmpersandCharacterCodeStart = notAppersandCode;  //Treat ampersand character codes as single characters.

            for (var i = 0; i < html.Length; i++)
            {
                var hmtlChar = html[i];
                
                switch (hmtlChar)
                {
                    case '<':
                        {
                            isHtmlTag = true;
                            bool isCloseTag, isSoloTag;
                            int? closePostion;
                            var tagTypeName = TagInfoViaLookahead(html, i, out isCloseTag, out isSoloTag, out closePostion);

                            if (!isCloseTag)
                            {
                                //Add tag to stack.                    
                                tags.Push(new TagItem(tagTypeName, i, closePostion));
                            }
                            else
                            {
                                //Remove from the tag if closed properly
                                if (tags.Peek().TagTypeName == tagTypeName) tags.Pop();
                            }
                        }
                        break;
                    case '>':
                        isHtmlTag = false;
                        break;
                    case '&' :
                        AmpersandCharacterCodeStart = i;
                        break;
                    case ';':
                        AmpersandCharacterCodeStart = notAppersandCode;
                        break;
                }


                truncatedHtml += hmtlChar;
                if (!isHtmlTag && hmtlChar != '>' && hmtlChar != '>' && AmpersandCharacterCodeStart == notAppersandCode) contentCount++;
                if (contentCount == contentLength) break;
            }
                       
            truncatedHtml = RemoveTagsLeftUnclosedByTheTruncation(truncatedHtml, tags);

            return truncatedHtml;
        }

        private static string RemoveTagsLeftUnclosedByTheTruncation(string truncatedHtml, IEnumerable<TagItem> tags)
        {
            const int includingTagStartAndEndOrStartCloseCharacters = 2; 
            foreach (var tag in tags)
            {
                // Note: when TagTypeName == "x" we are dealing with the 
                // last unfinished close tag that was not closed e.g. </b at the end of the html string.

                if (SoloTags.ContainsKey(tag.TagTypeName)) continue;

                var tagLength = tag.EndPositionInHtml.HasValue 
                                        ? tag.EndPositionInHtml.Value - tag.StartPositionInHtml + 1
                                        : tag.TagTypeName.Length + includingTagStartAndEndOrStartCloseCharacters;

                truncatedHtml = truncatedHtml.Remove(tag.StartPositionInHtml, tagLength);
            }
            return truncatedHtml;
        }

        /// <summary>
        /// Gets html tag name and sets a flag to record if it is an opening or closing tag
        /// and another to say if it is a solo tag e.g. <img />
        /// 
        /// Code from: http://www.dotnetperls.com/xhtml Thx.
        /// </summary>
        /// <returns>Entire tag name.</returns>
        private static string TagInfoViaLookahead(string html, int start, out bool isClose, out bool isSolo, out int? closePostion)
        {
            isClose = false;
            isSolo = false;

            var tagName = new StringBuilder();

            //
            // Stores the position of the final slash
            //
            int slashPos = -1;

            //
            // Whether we have encountered a space
            //
            bool space = false;

            //
            // Whether we are in a quote
            //
            bool quote = false;

            //
            // Position of the close character if closed
            //            
            closePostion = null;

            //
            // Begin scanning the tag
            //
            int i;
            for (i = 0; ; i++)
            {
                //
                // Get the position in main html
                //
                int pos = start + i;

                //
                // Don't go outside the html
                //
                if (pos >= html.Length)
                {
                    return "x";
                }

                //
                // The character we are looking at
                //
                char c = html[pos];

                //
                // See if a space has been encountered
                //
                if (char.IsWhiteSpace(c))
                {
                    space = true;
                }

                //
                // Add to our tag name if none of these are present
                //
                if (space == false &&
                    c != '<' &&
                    c != '>' &&
                    c != '/')
                {
                    tagName.Append(c);
                }

                //
                // Record position of slash if not inside a quoted area
                //
                if (c == '/' &&
                    quote == false)
                {
                    slashPos = i;
                }

                //
                // End at the > bracket
                //
                if (c == '>')
                {
                    closePostion = start + i;
                    break;
                }

                //
                // Record whether we are in a quoted area
                //
                if (c == '\"')
                {
                    quote = !quote;
                }
            }

            //
            // Determine if this is a solo or closing tag
            //
            if (slashPos != -1)
            {
                //
                // If slash is at the end so this is solo
                //
                if (slashPos + 1 == i)
                {
                    isSolo = true;
                }
                else
                {
                    isClose = true;
                }
            }

            //
            // Return the name of the tag collected
            //
            string name = tagName.ToString();
            if (name.Length == 0)
            {
                return "empty";
            }
            else
            {
                return name;
            }
        }

        /// <summary>Tags that must be closed in the start</summary>
        static readonly Dictionary<string, bool> SoloTags = new Dictionary<string, bool>()
        {
            {"img", true},
            {"br", true}
        };

    }
}