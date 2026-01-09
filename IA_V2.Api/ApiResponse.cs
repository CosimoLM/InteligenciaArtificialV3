using IA_V2.Core.CustomEntities;

namespace IA_V2.Api.Responses
{
    public class ApiResponse<T>
    {
        public Message[] Messages { get; set; }
        public Pagination Pagination { get; set; }
        public T Data { get; set; }
        public ApiResponse(T data)
        {
            Data = data;
        }
    }
}
