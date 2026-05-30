namespace PensionVault.Domain.Enums;

public enum UserRole { Member, Employer, FundAdmin, InvestmentOfficer, Compliance, Admin }
public enum UserStatus { Active, Retired, Inactive, Deceased }
public enum SchemeType { EPF, Gratuity, Superannuation, NPS, PPF }
public enum SchemeStatus { Active, Closed }
public enum EmployerStatus { Active, Defaulter, Deregistered }
public enum RemittanceFrequency { Monthly, Quarterly }
public enum MemberStatus { Active, Resigned, Retired, Deceased, Transferred }
public enum FundAccountStatus { Active, Settled, Frozen }
public enum RemittanceStatus { Received, Reconciled, Shortfall, Default }
public enum ContributionStatus { Posted, Reversed, Pending }
public enum EntryType { ContributionCredit, InterestCredit, PartialWithdrawal, TransferIn, TransferOut, ClaimDebit }
public enum LedgerEntryStatus { Posted, Reversed }
public enum InterestCreditStatus { Computed, Credited }
public enum ClaimType { Retirement, Resignation, PartialWithdrawal, DeathClaim, Disability, Marriage, Housing }
public enum ClaimStatus { Submitted, UnderReview, Approved, Rejected, Disbursed }
public enum DisbursementStatus { Pending, Processed, Failed }
public enum AssetClass { GovernmentSecurities, CorporateBonds, Equity, FixedDeposit, MoneyMarket }
public enum CorpusStatus { Draft, Finalised }
public enum AnnuityPlanType { LifeAnnuity, JointAnnuity, TemporaryAnnuity, GuaranteedAnnuity }
public enum AnnuityStatus { Active, Suspended, Lapsed, Settled }
public enum PensionDisbursementStatus { Pending, Disbursed, Failed }
public enum NotificationCategory { Contribution, Interest, Claim, Annuity, Compliance, Investment }
public enum NotificationStatus { Unread, Read, Dismissed }
