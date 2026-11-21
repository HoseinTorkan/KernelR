namespace KernelR.Application.Services.OtpService.Models;

/// <summary>
/// Defines the purpose of an OTP, used to select the appropriate
/// message template and validation rules.
/// </summary>
public enum OtpPurpose
{
    /// <summary>Unknown or unset purpose.</summary>
    Unknown = 0,

    /// <summary>User login/authentication.</summary>
    Login = 1,

    /// <summary>SuperAdmin registering a new Admin.</summary>
    RegisterAdmin = 2,

    /// <summary>Admin registering a new Seller.</summary>
    RegisterSeller = 3,

    /// <summary>Changing the user's phone number.</summary>
    ChangePhoneNumber = 4,

    /// <summary>Resetting a forgotten password.</summary>
    ResetPassword = 5,

    /// <summary>Two-factor authentication.</summary>
    TwoFactorAuth = 6,
}
