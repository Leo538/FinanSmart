namespace FinanSmart.Api.Common.Validation;

public static class EcuadorNationalIdValidator
{
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 10 || value.Any(character => character is < '0' or > '9')) return false;
        var digits = value.Select(character => character - '0').ToArray();
        var province = digits[0] * 10 + digits[1];
        if (province is < 1 or > 24 || digits[2] > 5) return false;
        var sum = 0;
        for (var index = 0; index < 9; index++)
        {
            var number = digits[index] * (index % 2 == 0 ? 2 : 1);
            sum += number > 9 ? number - 9 : number;
        }
        return (10 - sum % 10) % 10 == digits[9];
    }
}
