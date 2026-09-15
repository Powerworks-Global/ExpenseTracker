namespace CratisApp.Expenses.Approval.ApproveExpense;

using Cratis.Arc.Commands.ModelBound;
using Cratis.Chronicle.Events;
using Cratis.Chronicle.Keys;
using Cratis.Chronicle.Projections;
using Cratis.Monads;
using CratisApp.Expenses.Approval.RejectExpense;
using CratisApp.Expenses.Approval.SubmitExpense;
using CratisApp.Expenses.Payout.ConfirmPayout;
using CratisApp.Expenses.Payout.RequestPayout;

[ReadModel]
public record ExpenseStatus(
    Guid Id,
    string Status,
    decimal Amount,
    string Description,
    string Category,
    DateOnly Date,
    bool IsBusinessExpense,
    string? ReceiptUrl,
    string? ApprovedBy,
    string? ApprovalReason,
    string? TransactionReference,
    EventSourceId EventSourceId);

public class ExpenseStatusProjection : IProjectionFor<ExpenseStatus>
{
    public void Define(IProjectionBuilderFor<ExpenseStatus> builder) => builder
        .From<ExpenseSubmitted>(from => from
            .Set(m => m.Amount).To(e => e.Amount)
            .Set(m => m.Description).To(e => e.Description)
            .Set(m => m.Category).To(e => e.Category)
            .Set(m => m.Date).To(e => e.Date)
            .Set(m => m.IsBusinessExpense).To(e => e.IsBusinessExpense)
            .Set(m => m.ReceiptUrl).To(e => e.ReceiptUrl)
            .Set(m => m.Status).ToValue("PendingApproval"))
        .From<ExpenseApproved>(from => from
            .Set(m => m.ApprovedBy).To(e => e.ApprovedBy)
            .Set(m => m.ApprovalReason).To(e => e.Reason)
            .Set(m => m.Status).ToValue("Approved"))
        .From<ExpenseRejected>(from => from
            .Set(m => m.Status).ToValue("Rejected"))
        .From<ExpensePayoutRequested>(from => from
            .Set(m => m.Status).ToValue("PayoutRequested"))
        .From<ExpensePaid>(from => from
            .Set(m => m.TransactionReference).To(e => e.TransactionReference)
            .Set(m => m.Status).ToValue("Paid"));
}

[Command]
public record ApproveExpense([Key] Guid ExpenseId, string ApprovedBy, string Reason)
{
    // Design choice: ApproveExpense requires Reason parameter so AutoApprovalReactor can specify
    // "AutoThreshold" or "AutoNoGate", while server-side logic enforces "Manual" for non-system users.
    public Result<ExpenseApproved, string> Handle(ExpenseStatus? current)
    {
        if (current is null || current.Status != "PendingApproval")
        {
            return "Expense is not pending approval";
        }

        var effectiveReason = ApprovedBy == "system" ? Reason : "Manual";
        return new ExpenseApproved(ApprovedBy, effectiveReason);
    }
}

[EventType]
public record ExpenseApproved(string ApprovedBy, string Reason);
