using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

namespace DevTests
{
    public class DevTestDynamicSolution
    {
        /*
        Solution ProtobufCSDLLSolutionDynamic
        {
            get
            {
                string rootAbsolutePath = ((AbsolutePath) (RootDirectory /
                                                           "ExampleCustomProtoBufStructure/ExampleCustomProtoBufStructure.csproj"
                    ));
                rootAbsolutePath = PathConstruction.Combine(NukeBuild.RootDirectory, "ExampleCustomProtoBufStructure/ExampleCustomProtoBufStructure.csproj");
                //var solution = new Solution();
                //solution.Path = new DirectoryInfo(rootAbsolutePath);
                return ProjectModelTasks.ParseSolution(rootAbsolutePath);
            }
        }
        */

        public static Solution SolutionDynamic(AbsolutePath pathToProject)
        {
            return ProjectModelTasks.ParseSolution(pathToProject);
        }
        
    }
}
