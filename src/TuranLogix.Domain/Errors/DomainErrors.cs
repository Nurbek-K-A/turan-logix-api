namespace TuranLogix.Domain.Errors;

/// <summary>
/// Статические доменные ошибки, сгруппированные по агрегатам
/// </summary>
public static class DomainErrors
{
    /// <summary>
    /// Ошибки агрегата <c>User</c>
    /// </summary>
    public static class User
    {
        /// <summary>Пользователь не найден</summary>
        public static readonly Error NotFound = new("User.NotFound", "Пользователь не найден");

        /// <summary>Email уже занят другим пользователем</summary>
        public static readonly Error EmailAlreadyExists = new("User.EmailAlreadyExists", "Пользователь с таким email уже существует");

        /// <summary>Неверная пара email/пароль</summary>
        public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Неверный email или пароль");

        /// <summary>Аккаунт не прошёл верификацию</summary>
        public static readonly Error NotVerified = new("User.NotVerified", "Аккаунт не верифицирован");
    }

    /// <summary>
    /// Ошибки агрегата <c>Order</c>
    /// </summary>
    public static class Order
    {
        /// <summary>Заказ не найден</summary>
        public static readonly Error NotFound = new("Order.NotFound", "Заказ не найден");

        /// <summary>Клиент пытается получить чужой заказ</summary>
        public static readonly Error AccessDenied = new("Order.AccessDenied", "Нет доступа к этому заказу");

        /// <summary>Запрашиваемый переход статуса недопустим</summary>
        public static readonly Error InvalidStatusTransition = new("Order.InvalidStatusTransition", "Недопустимый переход статуса");
    }

    /// <summary>
    /// Ошибки OTP-верификации телефона
    /// </summary>
    public static class Phone
    {
        /// <summary>Неверный формат номера телефона (ожидается E.164)</summary>
        public static readonly Error InvalidFormat = new("Phone.InvalidFormat", "Неверный формат номера телефона. Используйте формат E.164, например: +77001234567");

        /// <summary>Не удалось отправить OTP-код</summary>
        public static readonly Error OtpSendFailed = new("Phone.OtpSendFailed", "Не удалось отправить код подтверждения");

        /// <summary>Верификация OTP не удалась (неверный или истёкший код)</summary>
        public static readonly Error OtpVerificationFailed = new("Phone.OtpVerificationFailed", "Код подтверждения неверен или истёк");

        /// <summary>Номер телефона не указан в профиле пользователя</summary>
        public static readonly Error PhoneNotSet = new("Phone.PhoneNotSet", "Номер телефона не указан в профиле");

        /// <summary>verifyId команды не совпадает с сохранённым — недействительная сессия</summary>
        public static readonly Error InvalidVerifySession = new("Phone.InvalidVerifySession", "Недействительная сессия верификации");

        /// <summary>Номер телефона был изменён после отправки OTP</summary>
        public static readonly Error PhoneChangedAfterOtp = new("Phone.PhoneChangedAfterOtp", "Номер телефона был изменён после отправки OTP");

        /// <summary>Время действия OTP-сессии истекло</summary>
        public static readonly Error OtpSessionExpired = new("Phone.OtpSessionExpired", "Сессия OTP истекла");
    }

    /// <summary>
    /// Ошибки агрегата <c>Document</c>
    /// </summary>
    public static class Document
    {
        /// <summary>Документ не найден</summary>
        public static readonly Error NotFound = new("Document.NotFound", "Документ не найден");

        /// <summary>Документ уже имеет ЭЦП-подпись</summary>
        public static readonly Error AlreadySigned = new("Document.AlreadySigned", "Документ уже подписан");

        /// <summary>Не удалось загрузить файл в хранилище</summary>
        public static readonly Error UploadFailed = new("Document.UploadFailed", "Ошибка загрузки файла");
    }
}

/// <summary>
/// Описание ошибки: код и текст сообщения
/// </summary>
/// <param name="Code">Машинный код ошибки</param>
/// <param name="Message">Читаемое сообщение об ошибке</param>
public record Error(string Code, string Message)
{
    /// <summary>Значение, обозначающее отсутствие ошибки</summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}
