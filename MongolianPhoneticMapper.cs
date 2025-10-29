using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MongolianPhonetic
{
    public class MongolianPhoneticMapper
    {
        private char? _lastChar = null;
        private DateTime _lastKeyTime = DateTime.MinValue;
        private const int COMBINATION_TIMEOUT_MS = 1000;

        // Single character mappings
        private static readonly Dictionary<char, char> _lowerCaseMap = new Dictionary<char, char>
        {
            {'a', 'а'}, {'b', 'б'}, {'v', 'в'}, {'g', 'г'}, {'d', 'д'},
            {'e', 'э'}, {'j', 'ж'}, {'z', 'з'}, {'i', 'и'}, {'k', 'к'},
            {'l', 'л'}, {'m', 'м'}, {'n', 'н'}, {'o', 'о'}, {'p', 'п'},
            {'r', 'р'}, {'s', 'с'}, {'t', 'т'}, {'u', 'у'}, {'f', 'ф'},
            {'h', 'х'}, {'x', 'х'}, {'c', 'ц'}, {'y', 'ы'}, {'q', 'ө'},
            {'w', 'ү'}, {'\'', 'ь'}, {'"', 'ъ'}
        };

        private static readonly Dictionary<char, char> _upperCaseMap = new Dictionary<char, char>
        {
            {'A', 'А'}, {'B', 'Б'}, {'V', 'В'}, {'G', 'Г'}, {'D', 'Д'},
            {'E', 'Э'}, {'J', 'Ж'}, {'Z', 'З'}, {'I', 'И'}, {'K', 'К'},
            {'L', 'Л'}, {'M', 'М'}, {'N', 'Н'}, {'O', 'О'}, {'P', 'П'},
            {'R', 'Р'}, {'S', 'С'}, {'T', 'Т'}, {'U', 'У'}, {'F', 'Ф'},
            {'H', 'Х'}, {'X', 'Х'}, {'C', 'Ц'}, {'Y', 'Ы'}, {'Q', 'Ө'},
            {'W', 'Ү'}
        };

        public void Reset()
        {
            _lastChar = null;
            _lastKeyTime = DateTime.MinValue;
        }

        public KeyProcessResult ProcessKey(Keys key, bool shift)
        {
            var result = new KeyProcessResult
            {
                ShouldSuppress = false,
                OutputText = null,
                BackspacesToSend = 0
            };

            // Check if too much time has passed since last key
            if ((DateTime.Now - _lastKeyTime).TotalMilliseconds > COMBINATION_TIMEOUT_MS)
            {
                _lastChar = null;
            }

            char? inputChar = GetCharFromKey(key, shift);

            // Debug logging for quote keys
            int vkCode = (int)key;
            if (vkCode >= 0xBA && vkCode <= 0xDF) // Range that includes all OEM keys
            {
                System.Diagnostics.Debug.WriteLine($"Mapper: Special key VK={vkCode:X}, Key={key}, inputChar={inputChar}");
            }

            if (inputChar == null)
            {
                _lastChar = null;
                return result;
            }

            char c = inputChar.Value;
            _lastKeyTime = DateTime.Now;

            // Check for two-character combinations
            if (_lastChar.HasValue)
            {
                string combination = CheckCombination(_lastChar.Value, c, shift);
                if (combination != null)
                {
                    result.ShouldSuppress = true;
                    result.BackspacesToSend = 1; // Remove the previous character
                    result.OutputText = combination;
                    _lastChar = null;
                    return result;
                }
            }

            // Special handling for doubling ' and "
            if (_lastChar == 'ь' && c == '\'')
            {
                result.ShouldSuppress = true;
                result.BackspacesToSend = 1;
                result.OutputText = "Ь";
                _lastChar = null;
                return result;
            }

            if (_lastChar == 'ъ' && c == '"')
            {
                result.ShouldSuppress = true;
                result.BackspacesToSend = 1;
                result.OutputText = "Ъ";
                _lastChar = null;
                return result;
            }

            // Single character mapping
            char? output = GetMappedChar(c, shift);
            if (output.HasValue)
            {
                result.ShouldSuppress = true;
                result.OutputText = output.Value.ToString();
                _lastChar = output.Value;
                return result;
            }

            // No mapping, pass through
            _lastChar = null;
            return result;
        }

        private string CheckCombination(char first, char second, bool shift)
        {
            // ye -> е
            if (first == 'ы' && second == 'e') return "е";
            if (first == 'Ы' && second == 'e') return "Е";
            if (first == 'Ы' && second == 'E') return "Е";

            // yo -> ё
            if (first == 'ы' && second == 'o') return "ё";
            if (first == 'Ы' && second == 'o') return "Ё";
            if (first == 'Ы' && second == 'O') return "Ё";

            // ts -> ц
            if (first == 'т' && second == 's') return "ц";
            if (first == 'Т' && second == 's') return "Ц";
            if (first == 'Т' && second == 'S') return "Ц";

            // ch -> ч
            if (first == 'ц' && second == 'h') return "ч";
            if (first == 'Ц' && second == 'h') return "Ч";
            if (first == 'Ц' && second == 'H') return "Ч";

            // sh -> ш
            if (first == 'с' && second == 'h') return "ш";
            if (first == 'С' && second == 'h') return "Ш";
            if (first == 'С' && second == 'H') return "Ш";

            // yu -> ю
            if (first == 'ы' && second == 'u') return "ю";
            if (first == 'Ы' && second == 'u') return "Ю";
            if (first == 'Ы' && second == 'U') return "Ю";

            // ya -> я
            if (first == 'ы' && second == 'a') return "я";
            if (first == 'Ы' && second == 'a') return "Я";
            if (first == 'Ы' && second == 'A') return "Я";

            // ai -> ай
            if (first == 'а' && second == 'i') return "ай";
            if (first == 'А' && second == 'i') return "Ай";
            if (first == 'А' && second == 'I') return "АЙ";

            // ei -> эй
            if (first == 'э' && second == 'i') return "эй";
            if (first == 'Э' && second == 'i') return "Эй";
            if (first == 'Э' && second == 'I') return "ЭЙ";

            // oi -> ой
            if (first == 'о' && second == 'i') return "ой";
            if (first == 'О' && second == 'i') return "Ой";
            if (first == 'О' && second == 'I') return "ОЙ";

            // ui -> уй
            if (first == 'у' && second == 'i') return "уй";
            if (first == 'У' && second == 'i') return "Уй";
            if (first == 'У' && second == 'I') return "УЙ";

            // qi -> өй
            if (first == 'ө' && second == 'i') return "өй";
            if (first == 'Ө' && second == 'i') return "Өй";
            if (first == 'Ө' && second == 'I') return "ӨЙ";

            // wi -> үй
            if (first == 'ү' && second == 'i') return "үй";
            if (first == 'Ү' && second == 'i') return "Үй";
            if (first == 'Ү' && second == 'I') return "ҮЙ";

            // ii -> ий
            if (first == 'и' && second == 'i') return "ий";
            if (first == 'И' && second == 'i') return "Ий";
            if (first == 'И' && second == 'I') return "ИЙ";

            return null;
        }

        private char? GetMappedChar(char c, bool shift)
        {
            // For special characters like ' and ", always check lowercase map first
            // because shift was already handled when determining the character
            if (c == '\'' || c == '"')
            {
                if (_lowerCaseMap.ContainsKey(c))
                    return _lowerCaseMap[c];
                return null;
            }

            // For regular characters, use shift to determine which map
            if (shift)
            {
                if (_upperCaseMap.ContainsKey(c))
                    return _upperCaseMap[c];
            }
            else
            {
                if (_lowerCaseMap.ContainsKey(c))
                    return _lowerCaseMap[c];
            }
            return null;
        }

        private char? GetCharFromKey(Keys key, bool shift)
        {
            // Handle letters
            if (key >= Keys.A && key <= Keys.Z)
            {
                char c = (char)('a' + (key - Keys.A));
                return shift ? char.ToUpper(c) : c;
            }

            // Handle special keys - quote/apostrophe keys
            // Different keyboards may use different key codes
            int vkCode = (int)key;

            // Japanese keyboard: Shift+7 = ', Shift+2 = "
            if (shift)
            {
                if (vkCode == 0x37 || key == Keys.D7) // Key 7 with Shift = '
                {
                    return '\'';
                }
                if (vkCode == 0x32 || key == Keys.D2) // Key 2 with Shift = "
                {
                    return '"';
                }
            }

            // US keyboard: OemQuotes key
            if (vkCode == 0xDE || vkCode == 0xC0 || vkCode == 0xBA)
            {
                // 0xDE = OemQuotes/Oem7 (' " key on US keyboard)
                // 0xC0 = Oemtilde (` ~ key, sometimes used for quotes on other layouts)
                // 0xBA = OemSemicolon (; : key, sometimes used on other layouts)
                return shift ? '"' : '\'';
            }

            return null;
        }
    }
}
