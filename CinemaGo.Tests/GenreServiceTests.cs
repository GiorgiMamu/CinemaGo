using CinemaGo.Application.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
namespace CinemaGo.Tests;

public class GenreServiceTests
{
    private readonly IGenreRepository _repo = Substitute.For<IGenreRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly GenreService _sut;

    public GenreServiceTests() => _sut = new GenreService(_repo, _uow, TestData.Mapper(), NullLogger<GenreService>.Instance);

    [Fact]
    public async Task Delete_GenreUsedByMovies_ThrowsConflict_AndDeletesNothing()
    {
        _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new Genre { Id = 1, Name = "Action" });
        _repo.IsUsedAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.DeleteAsync(1, TestContext.Current.CancellationToken));

        Assert.Equal(Errors.Genre.InUse.Code, ex.ErrorCode);
        _repo.DidNotReceive().Remove(Arg.Any<Genre>());
    }

    [Fact]
    public async Task Delete_UnusedGenre_IsRemoved()
    {
        var genre = new Genre { Id = 2, Name = "Western" };
        _repo.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(genre);
        _repo.IsUsedAsync(2, Arg.Any<CancellationToken>()).Returns(false);

        await _sut.DeleteAsync(2, TestContext.Current.CancellationToken);

        _repo.Received(1).Remove(genre);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}