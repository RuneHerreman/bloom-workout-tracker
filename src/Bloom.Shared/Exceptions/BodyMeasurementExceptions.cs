namespace Bloom.Shared.Exceptions;

public sealed class InvalidBodyMeasurementException(string message) : BloomGeneralException(message) { }
public sealed class BodyMeasurementNotFoundException(string message) : Exception(message) { }
