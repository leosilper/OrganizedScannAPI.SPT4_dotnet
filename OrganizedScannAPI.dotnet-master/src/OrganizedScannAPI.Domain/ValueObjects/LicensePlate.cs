namespace OrganizedScannApi.Domain.ValueObjects
{
    public readonly struct LicensePlate
    {
        public string Value { get; }
        public LicensePlate(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 6)
                throw new ArgumentException("Placa inválida.", nameof(value));
            Value = value.Trim().ToUpperInvariant();
        }
        public override string ToString() => Value;
    }
}
