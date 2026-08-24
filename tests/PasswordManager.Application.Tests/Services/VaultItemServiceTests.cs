using FluentAssertions;
using Moq;
using PasswordManager.Application.DTOs.VaultItem;
using PasswordManager.Application.Exceptions;
using PasswordManager.Application.Interfaces.Repositories;
using PasswordManager.Application.Services;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Tests.Services;

public class VaultItemServiceTests
{
    private readonly Mock<IVaultItemRepository> _vaultItemRepositoryMock;
    private readonly VaultItemService _sut;

    public VaultItemServiceTests()
    {
        _vaultItemRepositoryMock = new Mock<IVaultItemRepository>();
        _sut = new VaultItemService(_vaultItemRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesItemAndReturnsResponse()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var item = new CreateVaultItemRequest("encrypted-data", "nonce-value");

        //Act
        var result = await _sut.CreateAsync(userId, item);

        //Assert
        result.EncryptedData.Should().Be(item.EncryptedData);
        result.Nonce.Should().Be(item.Nonce);
        _vaultItemRepositoryMock.Verify(
        r => r.AddAsync(It.Is<VaultItem>(v =>
            v.UserId == userId &&
            v.EncryptedData == item.EncryptedData &&
            v.Nonce == item.Nonce)),
        Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllItemsForUser()
    {
        var userId = Guid.NewGuid();
        var items = new List<VaultItem>
    {
        new VaultItem { UserId = userId, EncryptedData = "data-1", Nonce = "nonce-1" },
        new VaultItem { UserId = userId, EncryptedData = "data-2", Nonce = "nonce-2" }
    };

        _vaultItemRepositoryMock
            .Setup(r => r.GetAllByUserIdAsync(userId))
            .ReturnsAsync(items);

        var result = await _sut.GetAllAsync(userId);

        result.Should().HaveCount(2);
        result.Select(r => r.EncryptedData).Should().Contain("data-1");
        result.Select(r => r.EncryptedData).Should().Contain("data-2");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingItem_ReturnsItem()
    {
        var userId = Guid.NewGuid();
        var existingItem = new VaultItem { UserId = userId, EncryptedData = "encrypted-data", Nonce = "nonce-value" };

        _vaultItemRepositoryMock
            .Setup(r => r.GetByIdAsync(existingItem.Id, userId))
            .ReturnsAsync(existingItem);

        var result = await _sut.GetByIdAsync(existingItem.Id, userId);

        result.EncryptedData.Should().Be(existingItem.EncryptedData);
        result.Nonce.Should().Be(existingItem.Nonce);
    }

    [Fact]
    public async Task GetByIdAsync_ItemDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        _vaultItemRepositoryMock
            .Setup(r => r.GetByIdAsync(itemId, userId))
            .ReturnsAsync((VaultItem?)null);

        // Act
        Func<Task> act = () => _sut.GetByIdAsync(itemId, userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
    [Fact]
    public async Task UpdateAsync_ExistingItem_UpdatesFieldsAndReturnsResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var existingItem = new VaultItem
        {
            Id = itemId,
            UserId = userId,
            EncryptedData = "old-data",
            Nonce = "old-nonce"
        };
        var request = new UpdateVaultItemRequest("new-data", "new-nonce");

        _vaultItemRepositoryMock
            .Setup(r => r.GetByIdAsync(itemId, userId))
            .ReturnsAsync(existingItem);

        // Act
        var result = await _sut.UpdateAsync(itemId, userId, request);

        // Assert
        result.EncryptedData.Should().Be("new-data");
        result.Nonce.Should().Be("new-nonce");
        result.ModifiedAt.Should().NotBeNull();

        _vaultItemRepositoryMock
            .Verify(r => r.UpdateAsync(existingItem), Times.Once);
    }
    [Fact]
    public async Task UpdateAsync_ItemDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var request = new UpdateVaultItemRequest("new-data", "new-nonce");

        _vaultItemRepositoryMock
            .Setup(r => r.GetByIdAsync(itemId, userId))
            .ReturnsAsync((VaultItem?)null);

        // Act
        Func<Task> act = () => _sut.UpdateAsync(itemId, userId, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _vaultItemRepositoryMock
            .Verify(
                r => r.UpdateAsync(It.IsAny<VaultItem>()),
                Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingItem_CallsRepositoryDelete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var existingItem = new VaultItem
        {
            Id = itemId,
            UserId = userId,
            EncryptedData = "data",
            Nonce = "nonce"
        };

        _vaultItemRepositoryMock
            .Setup(r => r.GetByIdAsync(itemId, userId))
            .ReturnsAsync(existingItem);

        // Act
        await _sut.DeleteAsync(itemId, userId);

        // Assert
        _vaultItemRepositoryMock
            .Verify(r => r.DeleteAsync(existingItem), Times.Once);
    }
    [Fact]
    public async Task DeleteAsync_ItemDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        _vaultItemRepositoryMock
            .Setup(r => r.GetByIdAsync(itemId, userId))
            .ReturnsAsync((VaultItem?)null);

        // Act
        Func<Task> act = () => _sut.DeleteAsync(itemId, userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _vaultItemRepositoryMock
            .Verify(
                r => r.DeleteAsync(It.IsAny<VaultItem>()),
                Times.Never);
    }
}