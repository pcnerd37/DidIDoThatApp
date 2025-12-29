using DidIDoThatApp.Helpers;
using DidIDoThatApp.Models;
using DidIDoThatApp.Models.Enums;
using TaskStatus = DidIDoThatApp.Models.Enums.TaskStatus;

namespace DidIDoThatApp.Tests.Helpers;

public class StatusCalculatorTests
{
    #region CalculateDueDate Tests

    [Fact]
    public void CalculateDueDate_WhenNeverCompleted_ReturnsNull()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 7, frequencyUnit: FrequencyUnit.Days);

        // Act
        var result = StatusCalculator.CalculateDueDate(task, lastCompletedDate: null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateDueDate_WithDaysFrequency_ReturnsCorrectDate()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 7, frequencyUnit: FrequencyUnit.Days);
        var lastCompleted = new DateTime(2024, 1, 1);

        // Act
        var result = StatusCalculator.CalculateDueDate(task, lastCompleted);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 8));
    }

    [Fact]
    public void CalculateDueDate_WithWeeksFrequency_ReturnsCorrectDate()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 2, frequencyUnit: FrequencyUnit.Weeks);
        var lastCompleted = new DateTime(2024, 1, 1);

        // Act
        var result = StatusCalculator.CalculateDueDate(task, lastCompleted);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 15));
    }

    [Fact]
    public void CalculateDueDate_WithMonthsFrequency_ReturnsCorrectDate()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 3, frequencyUnit: FrequencyUnit.Months);
        var lastCompleted = new DateTime(2024, 1, 1);

        // Act
        var result = StatusCalculator.CalculateDueDate(task, lastCompleted);

        // Assert
        // 3 months = 90 days (approximate)
        result.Should().Be(new DateTime(2024, 3, 31));
    }

    #endregion

    #region CalculateStatus Tests

    [Fact]
    public void CalculateStatus_WhenNeverCompleted_ReturnsOverdue()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 7, frequencyUnit: FrequencyUnit.Days);
        var now = DateTime.Now;

        // Act
        var result = StatusCalculator.CalculateStatus(task, lastCompletedDate: null, now);

        // Assert
        result.Should().Be(TaskStatus.Overdue);
    }

    [Fact]
    public void CalculateStatus_WhenDueDateInPast_ReturnsOverdue()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 7, frequencyUnit: FrequencyUnit.Days);
        var lastCompleted = new DateTime(2024, 1, 1);
        var now = new DateTime(2024, 1, 15); // 14 days after, due date was Jan 8

        // Act
        var result = StatusCalculator.CalculateStatus(task, lastCompleted, now);

        // Assert
        result.Should().Be(TaskStatus.Overdue);
    }

    [Fact]
    public void CalculateStatus_WhenDueDateFarInFuture_ReturnsUpToDate()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 30, frequencyUnit: FrequencyUnit.Days);
        var lastCompleted = new DateTime(2024, 1, 1);
        var now = new DateTime(2024, 1, 5); // 4 days after, due Jan 31, 20% = 6 days, so due soon starts Jan 25

        // Act
        var result = StatusCalculator.CalculateStatus(task, lastCompleted, now);

        // Assert
        result.Should().Be(TaskStatus.UpToDate);
    }

    [Fact]
    public void CalculateStatus_WhenWithin20PercentOfFrequency_ReturnsDueSoon()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 10, frequencyUnit: FrequencyUnit.Days);
        var lastCompleted = new DateTime(2024, 1, 1);
        // Due date = Jan 11
        // 20% of 10 days = 2 days
        // Due soon window starts Jan 9
        var now = new DateTime(2024, 1, 10); // Within the 20% window

        // Act
        var result = StatusCalculator.CalculateStatus(task, lastCompleted, now);

        // Assert
        result.Should().Be(TaskStatus.DueSoon);
    }

    [Fact]
    public void CalculateStatus_ExactlyOnDueDate_ReturnsDueSoon()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 7, frequencyUnit: FrequencyUnit.Days);
        var lastCompleted = new DateTime(2024, 1, 1);
        var now = new DateTime(2024, 1, 8); // Exactly on due date

        // Act
        var result = StatusCalculator.CalculateStatus(task, lastCompleted, now);

        // Assert
        result.Should().Be(TaskStatus.DueSoon);
    }

    #endregion

    #region GetNotificationLeadTime Tests

    [Theory]
    [InlineData(7, FrequencyUnit.Days, 3)] // Weekly = 3 days lead
    [InlineData(14, FrequencyUnit.Days, 3)] // 2 weeks = 3 days lead
    [InlineData(15, FrequencyUnit.Days, 7)] // 15 days = 7 days lead
    [InlineData(1, FrequencyUnit.Months, 7)] // Monthly = 7 days lead
    [InlineData(1, FrequencyUnit.Weeks, 3)] // Weekly = 3 days lead
    [InlineData(2, FrequencyUnit.Weeks, 3)] // 2 weeks = 3 days lead
    [InlineData(3, FrequencyUnit.Weeks, 7)] // 3 weeks = 7 days lead
    public void GetNotificationLeadTime_ReturnsCorrectLeadTime(
        int frequencyValue, FrequencyUnit frequencyUnit, int expectedLeadDays)
    {
        // Arrange
        var task = CreateTask(frequencyValue, frequencyUnit);

        // Act
        var result = StatusCalculator.GetNotificationLeadTime(task);

        // Assert
        result.Should().Be(TimeSpan.FromDays(expectedLeadDays));
    }

    #endregion

    #region CalculateNotificationTime Tests

    [Fact]
    public void CalculateNotificationTime_WhenDueDateNull_ReturnsNull()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 7, frequencyUnit: FrequencyUnit.Days);

        // Act
        var result = StatusCalculator.CalculateNotificationTime(task, dueDate: null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateNotificationTime_WhenReminderDisabled_ReturnsNull()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 7, frequencyUnit: FrequencyUnit.Days);
        task.IsReminderEnabled = false;
        var dueDate = DateTime.Now.AddDays(10);

        // Act
        var result = StatusCalculator.CalculateNotificationTime(task, dueDate);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateNotificationTime_WhenNotificationTimeInPast_ReturnsNull()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 7, frequencyUnit: FrequencyUnit.Days);
        var dueDate = DateTime.Now.AddDays(1); // Due tomorrow, lead time is 3 days

        // Act
        var result = StatusCalculator.CalculateNotificationTime(task, dueDate);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateNotificationTime_WithValidFutureDueDate_ReturnsCorrectTime()
    {
        // Arrange
        var task = CreateTask(frequencyValue: 7, frequencyUnit: FrequencyUnit.Days);
        var dueDate = DateTime.Now.AddDays(10); // Due in 10 days, lead time is 3 days

        // Act
        var result = StatusCalculator.CalculateNotificationTime(task, dueDate);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeCloseTo(dueDate.AddDays(-3), TimeSpan.FromSeconds(1));
    }

    #endregion

    #region GetDueDescription Tests

    [Fact]
    public void GetDueDescription_WhenNeverCompleted_ReturnsNeverCompleted()
    {
        // Act
        var result = StatusCalculator.GetDueDescription(null, DateTime.Now);

        // Assert
        result.Should().Be("Never completed");
    }

    [Fact]
    public void GetDueDescription_WhenDueToday_ReturnsDueToday()
    {
        // Arrange
        var now = new DateTime(2024, 1, 15, 12, 0, 0);
        var dueDate = new DateTime(2024, 1, 15, 18, 0, 0); // Later today

        // Act
        var result = StatusCalculator.GetDueDescription(dueDate, now);

        // Assert
        result.Should().Be("Due today");
    }

    [Fact]
    public void GetDueDescription_WhenDueTomorrow_ReturnsDueTomorrow()
    {
        // Arrange
        var now = new DateTime(2024, 1, 15);
        var dueDate = new DateTime(2024, 1, 16);

        // Act
        var result = StatusCalculator.GetDueDescription(dueDate, now);

        // Assert
        result.Should().Be("Due tomorrow");
    }

    [Fact]
    public void GetDueDescription_WhenDueInFuture_ReturnsDueInDays()
    {
        // Arrange
        var now = new DateTime(2024, 1, 15);
        var dueDate = new DateTime(2024, 1, 20);

        // Act
        var result = StatusCalculator.GetDueDescription(dueDate, now);

        // Assert
        result.Should().Be("Due in 5 days");
    }

    [Fact]
    public void GetDueDescription_WhenOverdue1Day_Returns1DayOverdue()
    {
        // Arrange
        var now = new DateTime(2024, 1, 16);
        var dueDate = new DateTime(2024, 1, 15);

        // Act
        var result = StatusCalculator.GetDueDescription(dueDate, now);

        // Assert
        result.Should().Be("1 day overdue");
    }

    [Fact]
    public void GetDueDescription_WhenOverdueMultipleDays_ReturnsDaysOverdue()
    {
        // Arrange
        var now = new DateTime(2024, 1, 20);
        var dueDate = new DateTime(2024, 1, 15);

        // Act
        var result = StatusCalculator.GetDueDescription(dueDate, now);

        // Assert
        result.Should().Be("5 days overdue");
    }

    #endregion

    #region Helper Methods

    private static TaskItem CreateTask(
        int frequencyValue = 1,
        FrequencyUnit frequencyUnit = FrequencyUnit.Days,
        bool isReminderEnabled = true)
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Test Task",
            FrequencyValue = frequencyValue,
            FrequencyUnit = frequencyUnit,
            IsReminderEnabled = isReminderEnabled,
            CreatedDate = DateTime.UtcNow
        };
    }

    #endregion
}
