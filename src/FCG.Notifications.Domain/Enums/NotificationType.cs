using System.ComponentModel;

namespace FCG.Notifications.Domain.Enums;

public enum NotificationType
{
    [Description("Boas vindas")]
    Welcome = 1,
    [Description("Confirmação de compra")]
    PurchaseConfirmation = 2
}