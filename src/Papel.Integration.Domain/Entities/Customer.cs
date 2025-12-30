namespace Papel.Integration.Domain.AggregatesModel.ToDoAggregates.Entities;

using Common;
using Enums;
using Papel.Integration.Events.Customer.EmailVerified;
using Papel.Integration.Events.Customer.Register;
using Papel.Integration.Events.Customer.Update;

[Table("Customer", Schema = "customer")]
public class Customer : WalletBaseTenantEntity
{
    [Column("CustomerId")]
    public long CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public long Tckn { get; set; }
    public DateTime BirthDate { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CustomerNo { get; set; } = string.Empty;
    public short? GenderId { get; set; }
    public short CustomerStatusId { get; set; }

    [Column("Password")]
    public string? PasswordHash { get; set; } = string.Empty;
    public bool IsSessionCheckNecessary { get; set; }
    public short WrongPasswordCount { get; set; }
    public DateTime? WrongPasswordLastDate { get; set; }
    public bool WrongPasswordBlocked { get; set; }
    public short WrongPasswordStepTwoCount { get; set; }
    public DateTime? WrongPasswordStepTwoLastDate { get; set; }
    public bool WrongPasswordStepTwoBlocked { get; set; }
    public DateTime? LastForgotPasswordDate { get; set; }
    public short WrongForgotPasswordCount { get; set; }
    public DateTime? WrongForgotPasswordLastDate { get; set; }
    public short CustomerSegmentId { get; set; }
    public DateTime? CustomerSegmentModifDate { get; set; }
    public bool HasSecretQuestionAnswer { get; set; }
    public bool IsEmailAddressVerified { get; set; }
    public DateTime? EmailAddressVerificationDate { get; set; }
    public bool SeenFailLoginAttempts { get; set; }
    public bool IsPendingApproval { get; set; }
    public short WrongAnswerCount { get; set; }
    public string? Nationality { get; set; } = string.Empty;
    public string? BucketName { get; set; }
    public string? ImageKey { get; set; }
    public bool IsKpsVerified { get; set; }
    public bool IsAddressVerified { get; set; }
    public long? MerchantId { get; set; }
    public string? ReferenceCode { get; set; }
    public long? ReferredById { get; set; }
    public string? SecurityImageUrl { get; set; }
    public bool IsPapelCustomer { get; set; }

    // Navigation Properties
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    // Business Methods with Domain Events
    public void RegisterCustomer()
    {
        AddDomainEvent(new CustomerRegisteredEvent(CustomerId, Email, FirstName, LastName));
    }

    public void UpdateCustomerInfo(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId;
        ModifDate = DateTime.UtcNow;

        AddDomainEvent(new CustomerUpdatedEvent(CustomerId, firstName, lastName, email));
    }

    public void VerifyEmail()
    {
        IsEmailAddressVerified = true;
        EmailAddressVerificationDate = DateTime.UtcNow;
        ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId;
        ModifDate = DateTime.UtcNow;

        AddDomainEvent(new CustomerEmailVerifiedEvent(CustomerId, Email));
    }
}
