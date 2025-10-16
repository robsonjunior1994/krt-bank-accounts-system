namespace KRT.BankAccounts.Api._01_Presentation.Helpers
{
    public static class ErrorMapper
    {
        public static int MapErrorToStatusCode(string errorCode)
        {
            return errorCode switch
            {
                ErrorCode.NOT_FOUND => StatusCodes.Status404NotFound,
                ErrorCode.VALIDATION_ERROR => StatusCodes.Status400BadRequest,
                ErrorCode.RESOURCE_ALREADY_EXISTS => StatusCodes.Status409Conflict,
                ErrorCode.DATABASE_ERROR => StatusCodes.Status500InternalServerError,
                ErrorCode.INTERNAL_ERROR => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
}
