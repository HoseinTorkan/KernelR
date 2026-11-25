using KernelR.Domain.Entities;

namespace KernelR.Domain.Tests.Entities;

public class BaseEntityTests
{
    [Fact]
    public void Constructor_WithValidId_ShouldInitializeEntity()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity = new TestEntity(id);

        // Assert
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void Constructor_WithDefaultGuid_ShouldThrowArgumentException()
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => new TestEntity(Guid.Empty));

        // Assert
        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithDefaultInt_ShouldThrowArgumentException()
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => new TestIntEntity(0));

        // Assert
        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Equals_WithSameReference_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.True(entity.Equals(entity));
    }

    [Fact]
    public void Equals_WithSameIdAndSameRuntimeType_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        var first = new TestEntity(id);
        var second = new TestEntity(id);

        // Act & Assert
        Assert.True(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentIds_ShouldReturnFalse()
    {
        // Arrange
        var first = new TestEntity(Guid.NewGuid());
        var second = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentRuntimeTypes_ShouldReturnFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        var first = new TestEntity(id);
        var second = new AnotherTestEntity(id);

        // Act & Assert
        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.False(entity.Equals(null));
    }

    [Fact]
    public void EqualsObject_WithSameIdentity_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        object first = new TestEntity(id);
        object second = new TestEntity(id);

        // Act & Assert
        Assert.True(first.Equals(second));
    }

    [Fact]
    public void EqualityOperators_WithSameIdentity_ShouldReturnExpectedResults()
    {
        // Arrange
        var id = Guid.NewGuid();

        var first = new TestEntity(id);
        var second = new TestEntity(id);

        // Act & Assert
        Assert.True(first == second);
        Assert.False(first != second);
    }

    [Fact]
    public void EqualityOperators_WithDifferentIdentity_ShouldReturnExpectedResults()
    {
        // Arrange
        var first = new TestEntity(Guid.NewGuid());
        var second = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.False(first == second);
        Assert.True(first != second);
    }

    [Fact]
    public void EqualityOperators_WithNull_ShouldReturnExpectedResults()
    {
        // Arrange
        TestEntity? first = null;
        TestEntity? second = null;

        // Act & Assert
        Assert.True(first == second);
        Assert.False(first != second);
    }

    [Fact]
    public void GetHashCode_WithSameIdentity_ShouldReturnSameHashCode()
    {
        // Arrange
        var id = Guid.NewGuid();

        var first = new TestEntity(id);
        var second = new TestEntity(id);

        // Act & Assert
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentRuntimeTypes_ShouldReturnConsistentWithEquality()
    {
        // Arrange
        var id = Guid.NewGuid();

        var first = new TestEntity(id);
        var second = new AnotherTestEntity(id);

        // Act & Assert
        Assert.False(first.Equals(second));

        // Note: Different entities are not required to have different hash codes.
    }

    [Fact]
    public void HashSet_WithSameIdentity_ShouldContainOnlyOneEntity()
    {
        // Arrange
        var id = Guid.NewGuid();

        var first = new TestEntity(id);
        var second = new TestEntity(id);

        var entities = new HashSet<TestEntity>();

        // Act
        entities.Add(first);
        entities.Add(second);

        // Assert
        Assert.Single(entities);
    }

    private sealed class TestEntity(Guid id) : BaseEntity<Guid>(id);

    private sealed class AnotherTestEntity(Guid id) : BaseEntity<Guid>(id);

    private sealed class TestIntEntity(int id) : BaseEntity<int>(id);
}
