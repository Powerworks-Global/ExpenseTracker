using Cratis.Monads;
using Cratis.Specifications;
using CratisApp.Expenses.Approval.ApproveExpense;
using CratisApp.Expenses.Payout.RequestPayout;

namespace CratisApp.Specs.for_RequestPayout;

public class when_expense_is_approved : Specification
{
    RequestPayout _command = null!;
    ExpenseStatus _current = null!;
    Result<ExpensePayoutRequested, string> _result = default!;

    void Establish()
    {
        var expenseId = Guid.NewGuid();
        _command = new(expenseId);
        _current = new(expenseId, "Approved", 42.00m, "Taxi", "Travel", DateOnly.FromDateTime(DateTime.UtcNow), false, null, "system", "AutoThreshold", null, expenseId);
    }

    void Because() => _result = _command.Handle(_current);

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
}
