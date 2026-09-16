using NUnit.Framework;
using UnityEngine;

public class GradeValidationServiceTests
{
    private GradeValidationConfig _config;
    private GradeValidationService _service;

    [SetUp]
    public void SetUp()
    {
        _config = ScriptableObject.CreateInstance<GradeValidationConfig>();
        _service = new GradeValidationService(_config);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_config);
    }

    [Test]
    public void Passes_GradeEqualsThreshold_ReturnsTrue()
    {
        var student = new StudentRecord { FinalGrade = 3.0f };
        Assert.IsTrue(_service.Passes(student));
    }

    [Test]
    public void Passes_GradeJustBelowThreshold_ReturnsFalse()
    {
        var student = new StudentRecord { FinalGrade = 2.99f };
        Assert.IsFalse(_service.Passes(student));
    }

    [Test]
    public void Passes_MinimumGrade_ReturnsFalse()
    {
        var student = new StudentRecord { FinalGrade = 0.0f };
        Assert.IsFalse(_service.Passes(student));
    }

    [Test]
    public void Passes_MaximumGrade_ReturnsTrue()
    {
        var student = new StudentRecord { FinalGrade = 5.0f };
        Assert.IsTrue(_service.Passes(student));
    }

    [Test]
    public void MatchesMarkedStatus_MarkedMatchesActualGrade_ReturnsTrue()
    {
        var student = new StudentRecord { FinalGrade = 4.0f };
        Assert.IsTrue(_service.MatchesMarkedStatus(student, markedAsApproved: true));
    }

    [Test]
    public void MatchesMarkedStatus_MarkedDoesNotMatchActualGrade_ReturnsFalse()
    {
        var student = new StudentRecord { FinalGrade = 4.0f };
        Assert.IsFalse(_service.MatchesMarkedStatus(student, markedAsApproved: false));
    }
}
