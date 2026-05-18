namespace Bloom.Application.Contracts.Ports;

public interface IUseCase<in Input, Output>
{
    Task<Output> Execute(Input input, CancellationToken ct = default);
}

public interface IUseCase<in Input>
{
    Task Execute(Input input, CancellationToken ct = default);
}
