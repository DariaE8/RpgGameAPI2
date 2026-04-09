using Xunit;
using RpgGame.Core.Models;

namespace RpgGame.Tests.UnitTests
{
    public class ItemModelTests
    {
        [Fact]
        public void Item_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var item = new Item();

            // Assert
            Assert.Equal(string.Empty, item.Name);
            Assert.Equal(string.Empty, item.Description);
            Assert.Equal(0, item.AttackModifier);
            Assert.Equal(0, item.HealthModifier);
            Assert.Equal(0, item.Value);
        }

[Theory]
[InlineData(ItemType.Weapon, true)]
[InlineData(ItemType.Armor, true)]
[InlineData(ItemType.Consumable, false)]
[InlineData(ItemType.Quest, true)] // Quest items should be equippable
public void IsEquippable_ShouldReturnCorrectValue(ItemType type, bool expected)
{
    // Arrange
    var item = new Item { Type = type };

    // Act & Assert
    Assert.Equal(expected, item.IsEquippable);
}

        [Fact]
        public void Item_ShouldInheritBaseEntityProperties()
        {
            // Arrange & Act
            var item = new Item();

            // Assert
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.True(item.CreatedAt <= DateTime.UtcNow);
            Assert.True(item.UpdatedAt <= DateTime.UtcNow);
        }
    }
}