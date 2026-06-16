namespace StockFlow.API.Models
{

    //T stands for "Type". It is just a placeholder that says "I don't know the data type yet — whoever uses this class will tell me."
    //For example: When returning a single category → ApiResponse<Category> — T becomes Category
    //When returning a list → ApiResponse<List<Category>> — T becomes List<Category>
    //When returning a string message → ApiResponse<string> — T becomes string
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public T? Data { get; set; }

        public List<string>? Errors { get; set; }

        public ApiResponse(T? data = default, bool success = true, string message = "Request successful.")
        {
            Data = data;
            Success = success;
            Message = message;
        }
        public static ApiResponse<T> SuccessResponse(T data, string message = "Request successful.")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default
            };
        }
        public static ApiResponse<T> ValidationErrorResponse(List<string> errors)
        {
            var response = new ApiResponse<T>
            {
                Success = false,
                Message = "Validation failed.",
                Data = default,
                Errors = errors
            };
            return response;
        }
    }
}