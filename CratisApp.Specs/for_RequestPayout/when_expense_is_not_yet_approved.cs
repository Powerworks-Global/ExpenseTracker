using Cratis.Monads;
using Cratis.Specifications;
using CratisApp.Expenses.Approval.ApproveExpense;
using CratisApp.Expenses.Payout.RequestPayout;

namespace CratisApp.Specs.for_RequestPayout;

public class when_expense_is_not_yet_approved : Specification
{
    RequestPayout _command = null!;
    Result<ExpensePayoutRequested, string> _result = default!;

    void Establish() => _command = new(Guid.NewGuid());

    void Because() => _result = _command.Handle(null);

    [Fact] void should_fail() => _result.IsSuccess.ShouldBeFalse();
}
