/**
 * Fix UTF-8 encoding issues (mojibake)
 * Converts mis-encoded UTF-8 characters back to proper UTF-8
 */
export function fixUTF8(str: string): string {
  if (!str) return str;

  // Common UTF-8 mojibake patterns (UTF-8 misinterpreted as Latin-1)
  const replacements: Record<string, string> = {
    'Ã©': 'é', // é
    'Ã¨': 'è', // è
    'Ãª': 'ê', // ê
    'Ã ': 'à', // à
    'Ã¢': 'â', // â
    'Ã´': 'ô', // ô
    'Ã»': 'û', // û
    'Ã§': 'ç', // ç
    'Ã®': 'î', // î
    'Ã¯': 'ï', // ï
    'Ã‰': 'É', // É
    'Ãˆ': 'È', // È
    'ÃŠ': 'Ê', // Ê
    'Ã€': 'À', // À
    'Ã‚': 'Â', // Â
    'Ã"': 'Ô', // Ô
    'Ã›': 'Û', // Û
    'Ã‡': 'Ç', // Ç
    'ÃŽ': 'Î', // Î
    'Å"': 'œ', // œ
    'Ã¦': 'æ', // æ
    'Ã±': 'ñ', // ñ
    'Ã¼': 'ü', // ü
    'Ã¶': 'ö', // ö
    'Ã¤': 'ä', // ä
    'Ãµ': 'õ', // õ
  };

  let fixed = str;
  for (const [pattern, replacement] of Object.entries(replacements)) {
    fixed = fixed.replace(new RegExp(pattern, 'g'), replacement);
  }

  return fixed;
}

/**
 * Fix encoding for an entire object recursively
 */
export function fixObjectEncoding(obj: any): any {
  if (typeof obj === 'string') {
    return fixUTF8(obj);
  } else if (Array.isArray(obj)) {
    return obj.map(item => fixObjectEncoding(item));
  } else if (obj !== null && typeof obj === 'object') {
    const fixed: any = {};
    for (const [key, value] of Object.entries(obj)) {
      fixed[key] = fixObjectEncoding(value);
    }
    return fixed;
  }
  return obj;
}