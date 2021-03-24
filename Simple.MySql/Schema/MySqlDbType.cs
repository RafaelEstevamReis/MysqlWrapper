namespace Simple.MySql.Schema
{
    public enum MySqlDbType
    {
        /// <summary>
        /// These types are synonyms for TINYINT(1). A value of zero is considered false. 
        /// Nonzero values are considered true
        /// </summary>
        Bool,
        /// <summary>
        ///  normal-sized integer that can be signed or unsigned. 
        ///  If signed, the allowable range is from -2147483648 to 2147483647. 
        ///  If unsigned, the allowable range is from 0 to 4294967295. 
        ///  You can specify a width of up to 11 digits.
        /// </summary>
        Int,
        /// <summary>
        /// A very small integer that can be signed or unsigned. 
        /// If signed, the allowable range is from -128 to 127. 
        /// If unsigned, the allowable range is from 0 to 255. 
        /// You can specify a width of up to 4 digits.
        /// </summary>
        TinyInt,
        /// <summary>
        /// A small integer that can be signed or unsigned. 
        /// If signed, the allowable range is from -32768 to 32767. 
        /// If unsigned, the allowable range is from 0 to 65535. 
        /// You can specify a width of up to 5 digits.
        /// </summary>
        SmallInt,
        /// <summary>
        /// A medium-sized integer that can be signed or unsigned. 
        /// If signed, the allowable range is from -8388608 to 8388607. 
        /// If unsigned, the allowable range is from 0 to 16777215. 
        /// You can specify a width of up to 9 digits.
        /// </summary>
        MediumInt,
        /// <summary>
        /// A large integer that can be signed or unsigned. 
        /// If signed, the allowable range is from -9223372036854775808 to 9223372036854775807. 
        /// If unsigned, the allowable range is from 0 to 18446744073709551615. 
        /// You can specify a width of up to 20 digits.
        /// </summary>
        BigInt,

        /// <summary>
        /// A floating-point number that cannot be unsigned. 
        /// You can define the display length (M) and the number of decimals (D). 
        /// This is not required and will default to 10,2, where 2 is the number of decimals and 10 is the total number of digits (including decimals). 
        /// Decimal precision can go to 24 places for a FLOAT.
        /// </summary>
        Float,
        /// <summary>
        /// A double precision floating-point number that cannot be unsigned. 
        /// You can define the display length (M) and the number of decimals (D). 
        /// This is not required and will default to 16,4, where 4 is the number of decimals. 
        /// Decimal precision can go to 53 places for a DOUBLE. 
        /// REAL is a synonym for DOUBLE.
        /// </summary>
        Double,
        /// <summary>
        /// An unpacked floating-point number that cannot be unsigned. 
        /// In the unpacked decimals, each decimal corresponds to one byte. 
        /// Defining the display length (M) and the number of decimals (D) is required. 
        /// NUMERIC is a synonym for DECIMAL.
        /// </summary>
        Decimal,

        /// <summary>
        /// A date in YYYY-MM-DD format, between 1000-01-01 and 9999-12-31. 
        /// For example, December 30th, 1973 would be stored as 1973-12-30.
        /// </summary>
        Date,
        /// <summary>
        /// A date and time combination in YYYY-MM-DD HH:MM:SS format, between 1000-01-01 00:00:00 and 9999-12-31 23:59:59. 
        /// For example, 3:30 in the afternoon on December 30th, 1973 would be stored as 1973-12-30 15:30:00.
        /// </summary>
        DateTime,
        /// <summary>
        /// A timestamp between midnight, January 1st, 1970 and sometime in 2037. 
        /// This looks like the previous DATETIME format, only without the hyphens between numbers; 
        /// 3:30 in the afternoon on December 30th, 1973 would be stored as 19731230153000 ( YYYYMMDDHHMMSS ).
        /// </summary>
        Timestamp,
        /// <summary>
        /// Stores the time in a HH:MM:SS format.
        /// </summary>
        Time,
        /// <summary>
        /// Stores a year in a 2-digit or a 4-digit format. 
        /// If the length is specified as 2 (for example YEAR(2)), YEAR can be between 1970 to 2069 (70 to 69). 
        /// If the length is specified as 4, then YEAR can be 1901 to 2155. 
        /// The default length is 4.
        /// </summary>
        Year,

        /// <summary>
        /// A fixed-length string between 1 and 255 characters in length (for example CHAR(5)), 
        /// right-padded with spaces to the specified length when stored. 
        /// Defining a length is not required, but the default is 1.
        /// </summary>
        Char,
        /// <summary>
        /// A variable-length string between 1 and 255 characters in length. 
        /// For example, VARCHAR(25). 
        /// You must define a length when creating a VARCHAR field.
        /// </summary>
        VarChar,

        /// <summary>
        /// A field with a maximum length of 65535 characters. 
        /// BLOBs are "Binary Large Objects" and are used to store large amounts of binary data, such as images or other types of files. 
        /// Fields defined as TEXT also hold large amounts of data. 
        /// The difference between the two is that the sorts and comparisons on the stored data are case sensitive on BLOBs and are not case sensitive in TEXT fields. 
        /// You do not specify a length with BLOB or TEXT.
        /// </summary>
        Text,
        /// <summary>
        /// A BLOB or TEXT column with a maximum length of 255 characters. 
        /// You do not specify a length with TINYBLOB or TINYTEXT.
        /// </summary>
        TinyText,
        /// <summary>
        /// A BLOB or TEXT column with a maximum length of 16777215 characters. 
        /// You do not specify a length with MEDIUMBLOB or MEDIUMTEXT.
        /// </summary>
        MediumText,
        /// <summary>
        /// A BLOB or TEXT column with a maximum length of 4294967295 characters. 
        /// You do not specify a length with LONGBLOB or LONGTEXT.
        /// </summary>
        LongText,

        /// <summary>
        /// A field with a maximum length of 65535 characters. 
        /// BLOBs are "Binary Large Objects" and are used to store large amounts of binary data, such as images or other types of files. 
        /// Fields defined as TEXT also hold large amounts of data. 
        /// The difference between the two is that the sorts and comparisons on the stored data are case sensitive on BLOBs and are not case sensitive in TEXT fields. 
        /// You do not specify a length with BLOB or TEXT.
        /// </summary>
        Blob,
        /// <summary>
        /// A BLOB or TEXT column with a maximum length of 255 characters. 
        /// You do not specify a length with TINYBLOB or TINYTEXT.
        /// </summary>
        TinyBlob,
        /// <summary>
        /// A BLOB or TEXT column with a maximum length of 16777215 characters. 
        /// You do not specify a length with MEDIUMBLOB or MEDIUMTEXT.
        /// </summary>
        MediumBlob,
        /// <summary>
        /// A BLOB or TEXT column with a maximum length of 4294967295 characters. 
        /// You do not specify a length with LONGBLOB or LONGTEXT.
        /// </summary>
        LongBlob,

        /// <summary>
        /// An enumeration, which is a fancy term for list. 
        /// When defining an ENUM, you are creating a list of items from which the value must be selected (or it can be NULL). 
        /// For example, if you wanted your field to contain "A" or "B" or "C", 
        /// you would define your ENUM as ENUM ('A', 'B', 'C') and only those values (or NULL) could ever populate that field.
        /// </summary>
        Enum
    }
}
