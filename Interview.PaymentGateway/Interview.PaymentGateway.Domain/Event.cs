namespace Interview.PaymentGateway.Domain;

public readonly record struct Event(object Message, string Topic);