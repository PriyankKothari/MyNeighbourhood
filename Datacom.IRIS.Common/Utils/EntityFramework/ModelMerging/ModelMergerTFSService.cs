using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Datacom.IRIS.Common.Utils.EntityFramework.ModelMerging
{
    public interface IModelMergerTFSService
    {
        void Checkout(string filepath);
        void SetWorkspacePath(string workspacePath);
    }

    public class ModelMergerTFSService : IModelMergerTFSService
    {
        private Workspace _workspace;

        public void SetWorkspacePath(string workspacePath)
        {
            var workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(workspacePath);
            _workspace = workspaceInfo.GetWorkspace(new TfsTeamProjectCollection(workspaceInfo.ServerUri));
        }
        public void Checkout(string filePath)
        {
            _workspace.Get(new GetRequest(filePath, RecursionType.None, VersionSpec.Latest), GetOptions.Overwrite);
            _workspace.PendEdit(filePath);
        }
    }
}