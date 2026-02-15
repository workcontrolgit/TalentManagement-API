namespace TalentManagementAPI.WebApi.Extensions
{
    // This class defines authorization policy constants for the application.
    public class AuthorizationConsts
    {
        // Constant for the Admin role policy
        public const string AdminPolicy = "AdminPolicy";

        // Constant for the Manager role policy
        public const string ManagerPolicy = "ManagerPolicy";

        // Constant for the Employee role policy
        public const string EmployeePolicy = "EmployeePolicy";

        // Capability policies for client-credential and mixed auth models
        public const string ApiReadPolicy = "ApiReadPolicy";
        public const string ApiWritePolicy = "ApiWritePolicy";
        public const string ApiAdminPolicy = "ApiAdminPolicy";
    }
}

