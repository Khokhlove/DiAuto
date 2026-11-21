using Microsoft.AspNetCore.Identity;

namespace DiAvto.Admin.Data;

public sealed class RussianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => new()
    {
        Code = nameof(DefaultError),
        Description = "Произошла неизвестная ошибка."
    };

    public override IdentityError ConcurrencyFailure() => new()
    {
        Code = nameof(ConcurrencyFailure),
        Description = "Данные были изменены другим пользователем. Обновите страницу и попробуйте снова."
    };

    public override IdentityError PasswordMismatch() => new()
    {
        Code = nameof(PasswordMismatch),
        Description = "Неверный пароль."
    };

    public override IdentityError InvalidToken() => new()
    {
        Code = nameof(InvalidToken),
        Description = "Неверный или просроченный токен."
    };

    public override IdentityError LoginAlreadyAssociated() => new()
    {
        Code = nameof(LoginAlreadyAssociated),
        Description = "Этот внешний вход уже привязан к другому аккаунту."
    };

    public override IdentityError InvalidUserName(string? userName) => new()
    {
        Code = nameof(InvalidUserName),
        Description = $"Недопустимое имя пользователя «{userName}»."
    };

    public override IdentityError InvalidEmail(string? email) => new()
    {
        Code = nameof(InvalidEmail),
        Description = $"Недопустимый адрес электронной почты «{email}»."
    };

    public override IdentityError DuplicateUserName(string? userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = $"Пользователь «{userName}» уже существует."
    };

    public override IdentityError DuplicateEmail(string? email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = $"Адрес «{email}» уже используется."
    };

    public override IdentityError InvalidRoleName(string? role) => new()
    {
        Code = nameof(InvalidRoleName),
        Description = $"Недопустимое название роли «{role}»."
    };

    public override IdentityError DuplicateRoleName(string? role) => new()
    {
        Code = nameof(DuplicateRoleName),
        Description = $"Роль «{role}» уже существует."
    };

    public override IdentityError UserAlreadyHasPassword() => new()
    {
        Code = nameof(UserAlreadyHasPassword),
        Description = "У пользователя уже задан пароль."
    };

    public override IdentityError UserLockoutNotEnabled() => new()
    {
        Code = nameof(UserLockoutNotEnabled),
        Description = "Для этого пользователя не включена блокировка."
    };

    public override IdentityError UserAlreadyInRole(string? role) => new()
    {
        Code = nameof(UserAlreadyInRole),
        Description = $"Пользователь уже имеет роль «{role}»."
    };

    public override IdentityError UserNotInRole(string? role) => new()
    {
        Code = nameof(UserNotInRole),
        Description = $"У пользователя нет роли «{role}»."
    };

    public override IdentityError PasswordTooShort(int length) => new()
    {
        Code = nameof(PasswordTooShort),
        Description = $"Пароль должен содержать не менее {length} символов."
    };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
    {
        Code = nameof(PasswordRequiresNonAlphanumeric),
        Description = "Пароль должен содержать хотя бы один специальный символ."
    };

    public override IdentityError PasswordRequiresDigit() => new()
    {
        Code = nameof(PasswordRequiresDigit),
        Description = "Пароль должен содержать хотя бы одну цифру."
    };

    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = "Пароль должен содержать хотя бы одну строчную букву."
    };

    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = "Пароль должен содержать хотя бы одну заглавную букву."
    };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new()
    {
        Code = nameof(PasswordRequiresUniqueChars),
        Description = $"Пароль должен содержать не менее {uniqueChars} разных символов."
    };

    public override IdentityError RecoveryCodeRedemptionFailed() => new()
    {
        Code = nameof(RecoveryCodeRedemptionFailed),
        Description = "Не удалось использовать резервный код."
    };
}

