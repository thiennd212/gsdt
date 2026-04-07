using GSDT.Cases.Domain.Entities;

namespace GSDT.Regression.Tests.Core;

/// <summary>
/// Regression tests for Case state machine — TC-REG-CORE-004.
/// Ensures workflow invariants (valid transition paths) are never broken.
/// </summary>
public sealed class CasesRegressionTests
{
    [Fact]
    [Trait("Category", "Regression")]
    [Trait("Module", "Cases")]
    public void Draft_Submit_TransitionsToSubmitted()
    {
        var result = CaseWorkflow.Engine.Execute(CaseStatus.Draft, CaseAction.Submit);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(CaseStatus.Submitted);
    }

    [Fact]
    [Trait("Category", "Regression")]
    [Trait("Module", "Cases")]
    public void Draft_CannotClose_DirectlySkippingReview()
    {
        var result = CaseWorkflow.Engine.Execute(CaseStatus.Draft, CaseAction.Close);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("not allowed");
    }

    [Fact]
    [Trait("Category", "Regression")]
    [Trait("Module", "Cases")]
    public void Draft_CannotApprove_DirectlySkippingSubmitAndReview()
    {
        var result = CaseWorkflow.Engine.Execute(CaseStatus.Draft, CaseAction.Approve);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Regression")]
    [Trait("Module", "Cases")]
    public void UnderReview_Return_TransitionsToReturnedForRevision()
    {
        var result = CaseWorkflow.Engine.Execute(CaseStatus.UnderReview, CaseAction.Return);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(CaseStatus.ReturnedForRevision);
    }

    [Fact]
    [Trait("Category", "Regression")]
    [Trait("Module", "Cases")]
    public void FullLifecycle_Draft_To_Closed_ValidPath()
    {
        // Draft → Submitted → UnderReview → Approved → Closed
        var s1 = CaseWorkflow.Engine.Execute(CaseStatus.Draft, CaseAction.Submit);
        s1.IsSuccess.Should().BeTrue();

        var s2 = CaseWorkflow.Engine.Execute(s1.Value, CaseAction.Assign);
        s2.IsSuccess.Should().BeTrue();

        var s3 = CaseWorkflow.Engine.Execute(s2.Value, CaseAction.Approve);
        s3.IsSuccess.Should().BeTrue();

        var s4 = CaseWorkflow.Engine.Execute(s3.Value, CaseAction.Close);
        s4.IsSuccess.Should().BeTrue();
        s4.Value.Should().Be(CaseStatus.Closed);
    }

    [Fact]
    [Trait("Category", "Regression")]
    [Trait("Module", "Cases")]
    public void ReturnedForRevision_CanResubmit()
    {
        var result = CaseWorkflow.Engine.Execute(
            CaseStatus.ReturnedForRevision, CaseAction.Submit);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(CaseStatus.Submitted);
    }

    [Fact]
    [Trait("Category", "Regression")]
    [Trait("Module", "Cases")]
    public void RejectedPath_Draft_To_Rejected_To_Closed()
    {
        var s1 = CaseWorkflow.Engine.Execute(CaseStatus.Draft, CaseAction.Submit);
        var s2 = CaseWorkflow.Engine.Execute(s1.Value, CaseAction.Assign);
        var s3 = CaseWorkflow.Engine.Execute(s2.Value, CaseAction.Reject);
        s3.IsSuccess.Should().BeTrue();
        s3.Value.Should().Be(CaseStatus.Rejected);

        var s4 = CaseWorkflow.Engine.Execute(s3.Value, CaseAction.Close);
        s4.IsSuccess.Should().BeTrue();
        s4.Value.Should().Be(CaseStatus.Closed);
    }
}
