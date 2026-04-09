using Xunit;
using FluentValidation.TestHelper;
using RpgGame.Core.DTOs;
using RpgGame.Core.Validators;

namespace RpgGame.Tests.UnitTests
{
    public class ValidatorTests
    {
        private readonly CreatePlayerDtoValidator _playerValidator;
        private readonly CreateEnemyDtoValidator _enemyValidator;
        private readonly CreateQuestDtoValidator _questValidator;
        private readonly CreateGameLocationDtoValidator _locationValidator;

        public ValidatorTests()
        {
            _playerValidator = new CreatePlayerDtoValidator();
            _enemyValidator = new CreateEnemyDtoValidator();
            _questValidator = new CreateQuestDtoValidator();
            _locationValidator = new CreateGameLocationDtoValidator();
        }

        [Fact]
        public void CreatePlayerDtoValidator_ShouldValidateValidPlayer()
        {
            // Arrange
            var player = new CreatePlayerDto
            {
                Name = "TestPlayer",
                Email = "test@example.com"
            };

            // Act
            var result = _playerValidator.TestValidate(player);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

[Theory]
[InlineData("VeryLongNameThatExceedsTheMaximumAllowedLength", false)]
[InlineData("A", false)]
[InlineData("ValidName", true)]
[InlineData("", false)]
public void CreatePlayerDtoValidator_ShouldValidateName(string name, bool expectedIsValid)
{
    // Arrange
    var validator = new CreatePlayerDtoValidator();
    var dto = new CreatePlayerDto { Name = name, Email = "test@test.com" };

    // Act
    var result = validator.Validate(dto);

    // Assert
    Assert.Equal(expectedIsValid, result.IsValid);
}

[Theory]
[InlineData("", "Email is required")]
[InlineData("invalid-email", "Valid email address is required")]
// Убрать тест с "missing@domain" - это валидный email
public void CreatePlayerDtoValidator_ShouldValidateEmail(string email, string expectedError)
{
    // Arrange
    var player = new CreatePlayerDto
    {
        Name = "TestPlayer",
        Email = email
    };

    // Act
    var result = _playerValidator.TestValidate(player);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Email)
          .WithErrorMessage(expectedError);
}

        [Fact]
        public void CreateEnemyDtoValidator_ShouldValidateValidEnemy()
        {
            // Arrange
            var enemy = new CreateEnemyDto
            {
                Name = "Goblin",
                Level = 5,
                Health = 50,
                MaxHealth = 50,
                Attack = 10,
                ExperienceReward = 25,
                GoldReward = 10,
                Location = "forest"
            };

            // Act
            var result = _enemyValidator.TestValidate(enemy);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0, "Level must be greater than 0")]
        [InlineData(101, "Level cannot exceed 100")]
        public void CreateEnemyDtoValidator_ShouldValidateLevel(int level, string expectedError)
        {
            // Arrange
            var enemy = new CreateEnemyDto
            {
                Name = "Goblin",
                Level = level,
                Health = 50,
                MaxHealth = 50
            };

            // Act
            var result = _enemyValidator.TestValidate(enemy);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Level)
                  .WithErrorMessage(expectedError);
        }

        [Fact]
        public void CreateEnemyDtoValidator_ShouldValidateMaxHealth()
        {
            // Arrange
            var enemy = new CreateEnemyDto
            {
                Name = "Goblin",
                Health = 60,
                MaxHealth = 50
            };

            // Act
            var result = _enemyValidator.TestValidate(enemy);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.MaxHealth)
                  .WithErrorMessage("Max health must be greater than or equal to current health");
        }

        [Fact]
        public void CreateQuestDtoValidator_ShouldValidateValidQuest()
        {
            // Arrange
            var quest = new CreateQuestDto
            {
                Title = "Test Quest",
                Description = "This is a test quest description",
                Objective = "Complete the test objective",
                TargetCount = 5,
                ExperienceReward = 100,
                GoldReward = 50
            };

            // Act
            var result = _questValidator.TestValidate(quest);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("", "Quest title is required")]
        [InlineData("Test", "Title must be between 5 and 100 characters")]
        public void CreateQuestDtoValidator_ShouldValidateTitle(string title, string expectedError)
        {
            // Arrange
            var quest = new CreateQuestDto
            {
                Title = title,
                Description = "Valid description",
                Objective = "Valid objective"
            };

            // Act
            var result = _questValidator.TestValidate(quest);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(expectedError);
        }

        [Fact]
        public void CreateGameLocationDtoValidator_ShouldValidateValidLocation()
        {
            // Arrange
            var location = new CreateGameLocationDto
            {
                Name = "Test Location",
                Description = "This is a test location description",
                RequiredLevel = 5,
                AvailableEnemies = new List<string>(),
                AvailableQuests = new List<Guid>()
            };

            // Act
            var result = _locationValidator.TestValidate(location);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0, "Required level must be at least 1")]
        [InlineData(101, "Required level cannot exceed 100")]
        public void CreateGameLocationDtoValidator_ShouldValidateRequiredLevel(int level, string expectedError)
        {
            // Arrange
            var location = new CreateGameLocationDto
            {
                Name = "Test Location",
                Description = "Valid description",
                RequiredLevel = level
            };

            // Act
            var result = _locationValidator.TestValidate(location);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RequiredLevel)
                  .WithErrorMessage(expectedError);
        }
    }
}