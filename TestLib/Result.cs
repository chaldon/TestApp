namespace TestApp
{
    public class Result
    {
        public static readonly Result SUCCESS = new Result(true, null);
        protected Result(bool result, string message)
        {
            Success = result;
            Message = message;
        }
        public bool Success { get; protected set; }
        public string Message { get; protected set; }

        public static Result Ok() => SUCCESS;
        public static Result Fail(string message) => new Result(false, message);

        public static implicit operator bool(Result d) => d.Success;
        public static implicit operator string(Result d) => d.Message;
    }
    public sealed class Result<T>: Result
    {
        public T Data { get; private set; }
        public Result(bool result, T data, string message): base(result, message)
        {
            Data = data;
        }

        public static Result<T> Ok(T data) => new Result<T>(true, data, null);
        public static new Result<T> Fail(string message) => new Result<T>(false, default, message);
    }
}
