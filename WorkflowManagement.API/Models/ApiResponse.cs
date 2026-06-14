namespace WorkflowManagement.API.Models
{


    public class ApiResponse<T>
    {
        /// <summary>
        /// Success status
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Response data
        /// </summary>
        public T? Data { get; }

        /// <summary>
        /// Response message
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// Success response constructor
        /// </summary>
        public ApiResponse(T data)
        {
            Success = true;

            Data = data;

            Message = null;
        }

        /// <summary>
        /// Error response constructor
        /// </summary>
        public ApiResponse(string message)
        {
            Success = false;

            Data = default;

            Message = message;
        }
    }



}
