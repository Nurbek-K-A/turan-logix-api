namespace TuranLogix.Domain.Errors;

public static class DomainErrors
{
    public static class User
    {
        public static readonly Error NotFound = new("User.NotFound", "Пользователь не найден");
        public static readonly Error EmailAlreadyExists = new("User.EmailAlreadyExists", "Пользователь с таким email уже существует");
        public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Неверный email или пароль");
        public static readonly Error NotVerified = new("User.NotVerified", "Аккаунт не верифицирован");
    }

    public static class Order
    {
        public static readonly Error NotFound = new("Order.NotFound", "Заказ не найден");
        public static readonly Error AccessDenied = new("Order.AccessDenied", "Нет доступа к этому заказу");
        public static readonly Error InvalidStatusTransition = new("Order.InvalidStatusTransition", "Недопустимый переход статуса");
    }

    public static class Document
    {
        public static readonly Error NotFound = new("Document.NotFound", "Документ не найден");
        public static readonly Error AlreadySigned = new("Document.AlreadySigned", "Документ уже подписан");
        public static readonly Error UploadFailed = new("Document.UploadFailed", "Ошибка загрузки файла");
    }
}

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
