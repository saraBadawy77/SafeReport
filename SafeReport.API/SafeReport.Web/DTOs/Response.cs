namespace SafeReport.Web.DTOs
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static Response<T> SuccessResponse(T data, string message = "Success")
            => new Response<T> { Success = true, Message = message, Data = data };

        public static Response<T> FailResponse(string message)
            => new Response<T> { Success = false, Message = message };

    }
}
