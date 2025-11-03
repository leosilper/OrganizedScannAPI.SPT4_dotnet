namespace OrganizedScannApi.Domain.ValueObjects
{
    public readonly struct Email
    {
        public string Value { get; }
        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
                throw new ArgumentException("Email inválido.", nameof(value));
            Value = value.Trim();
        }
        public override string ToString() => Value;
    }
}
