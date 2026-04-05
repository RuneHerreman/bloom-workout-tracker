namespace Bloom.Application.Contracts.Ports;

public interface IUseCase<in Input, Output>
{
    Task<Output> Execute(Input input);
}

public interface IUseCase<in Input>
{
    Task Execute(Input input);
}