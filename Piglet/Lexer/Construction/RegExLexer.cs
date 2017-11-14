using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Piglet.Lexer.Construction
{
    internal class RegExLexer
    {
        private readonly TextReader input;
        private State state;


        private enum State
        {
            Normal,
            NormalEscaped,
            BeginCharacterClass,
            InsideCharacterClass,
            RangeEnd,
            NumberedRepetition,
            InsideCharacterClassEscaped
        }

        private class CharacterClassState
        {
            public CharacterClassState()
            {
                CharsSet = new CharSet();
            }

            public CharSet CharsSet { get; private set; }
            public bool Negated { get; set; }
            public char LastChar { get; set; }
        }

        private class NumberedRepetitionState
        {
            public NumberedRepetitionState()
            {
                MinRepetitions = -1;
                MaxRepetitions = -1;
                Chars = new List<char>();
            }

            public int MaxRepetitions { get; set; }
            public int MinRepetitions { get; set; }
            public List<char> Chars { get; private set; }
            public int CurrentPart { get; set; }
        }


        public RegExLexer(TextReader input)
        {
            this.input = input;
            state = State.Normal;
        }

		private static readonly char[] escd = new[] 
			{
				'\u0030', '\u0039', '\u0660', '\u0669', '\u06f0', '\u06f9', '\u07c0', '\u07c9', '\u0966', '\u096f', '\u09e6', '\u09ef',
				'\u0a66', '\u0a6f', '\u0ae6', '\u0aef', '\u0b66', '\u0b6f', '\u0be6', '\u0bef', '\u0c66', '\u0c6f', '\u0ce6', '\u0cef',
				'\u0d66', '\u0d6f', '\u0e50', '\u0e59', '\u0ed0', '\u0ed9', '\u0f20', '\u0f29', '\u1040', '\u1049', '\u1090', '\u1099',
				'\u17e0', '\u17e9', '\u1810', '\u1819', '\u1946', '\u194f', '\u19d0', '\u19d9', '\u1b50', '\u1b59', '\u1bb0', '\u1bb9',
				'\u1c40', '\u1c49', '\u1c50', '\u1c59', '\ua620', '\ua629', '\ua8d0', '\ua8d9', '\ua900', '\ua909', '\uaa50', '\uaa59',
				'\uff10', '\uff19'
			};

		private static readonly char[] escD = new []
			{
				'\u0001', '\u002f', '\u003a', '\u065f', '\u066a', '\u06ef', '\u06fa', '\u07bf', '\u07ca', '\u0965', '\u0970', '\u09e5',
                '\u09f0', '\u0a65', '\u0a70', '\u0ae5', '\u0af0', '\u0b65', '\u0b70', '\u0be5', '\u0bf0', '\u0c65', '\u0c70', '\u0ce5',
                '\u0cf0', '\u0d65', '\u0d70', '\u0e4f', '\u0e5a', '\u0ecf', '\u0eda', '\u0f1f', '\u0f2a', '\u103f', '\u104a', '\u108f',
                '\u109a', '\u17df', '\u17ea', '\u180f', '\u181a', '\u1945', '\u1950', '\u19cf', '\u19da', '\u1b4f', '\u1b5a', '\u1baf',
                '\u1bba', '\u1c3f', '\u1c4a', '\u1c4f', '\u1c5a', '\ua61f', '\ua62a', '\ua8cf', '\ua8da', '\ua8ff', '\ua90a', '\uaa4f',
                '\uaa5a', '\uff0f'
			};

    	private static readonly char[] escw = new[]
    		{
				'\u0030', '\u0039', '\u0041', '\u005a', '\u005f', '\u005f', '\u0061', '\u007a', '\u00aa', '\u00aa', '\u00b5', '\u00b5',
                '\u00ba','\u00ba','\u00c0','\u00d6','\u00d8','\u00f6','\u00f8','\u02c1','\u02c6','\u02d1','\u02e0','\u02e4',
                '\u02ec','\u02ec','\u02ee','\u02ee','\u0300','\u0374','\u0376','\u0377','\u037a','\u037d','\u0386','\u0386',
                '\u0388','\u038a','\u038c','\u038c','\u038e','\u03a1','\u03a3','\u03f5','\u03f7','\u0481','\u0483','\u0487',
                '\u048a','\u0523','\u0531','\u0556','\u0559','\u0559','\u0561','\u0587','\u0591','\u05bd','\u05bf','\u05bf',
                '\u05c1','\u05c2','\u05c4','\u05c5','\u05c7','\u05c7','\u05d0','\u05ea','\u05f0','\u05f2','\u0610','\u061a',
                '\u0621','\u065e','\u0660','\u0669','\u066e','\u06d3','\u06d5','\u06dc','\u06df','\u06e8','\u06ea','\u06fc',
                '\u06ff','\u06ff','\u0710','\u074a','\u074d','\u07b1','\u07c0','\u07f5','\u07fa','\u07fa','\u0901','\u0902',
                '\u0904','\u0939','\u093c','\u093d','\u0941','\u0948','\u094d','\u094d','\u0950','\u0954','\u0958','\u0963',
                '\u0966','\u096f','\u0971','\u0972','\u097b','\u097f','\u0981','\u0981','\u0985','\u098c','\u098f','\u0990',
                '\u0993','\u09a8','\u09aa','\u09b0','\u09b2','\u09b2','\u09b6','\u09b9','\u09bc','\u09bd','\u09c1','\u09c4',
                '\u09cd','\u09ce','\u09dc','\u09dd','\u09df','\u09e3','\u09e6','\u09f1','\u0a01','\u0a02','\u0a05','\u0a0a',
                '\u0a0f','\u0a10','\u0a13','\u0a28','\u0a2a','\u0a30','\u0a32','\u0a33','\u0a35','\u0a36','\u0a38','\u0a39',
                '\u0a3c','\u0a3c','\u0a41','\u0a42','\u0a47','\u0a48','\u0a4b','\u0a4d','\u0a51','\u0a51','\u0a59','\u0a5c',
                '\u0a5e','\u0a5e','\u0a66','\u0a75','\u0a81','\u0a82','\u0a85','\u0a8d','\u0a8f','\u0a91','\u0a93','\u0aa8',
                '\u0aaa','\u0ab0','\u0ab2','\u0ab3','\u0ab5','\u0ab9','\u0abc','\u0abd','\u0ac1','\u0ac5','\u0ac7','\u0ac8',
                '\u0acd','\u0acd','\u0ad0','\u0ad0','\u0ae0','\u0ae3','\u0ae6','\u0aef','\u0b01','\u0b01','\u0b05','\u0b0c',
                '\u0b0f','\u0b10','\u0b13','\u0b28','\u0b2a','\u0b30','\u0b32','\u0b33','\u0b35','\u0b39','\u0b3c','\u0b3d',
                '\u0b3f','\u0b3f','\u0b41','\u0b44','\u0b4d','\u0b4d','\u0b56','\u0b56','\u0b5c','\u0b5d','\u0b5f','\u0b63',
                '\u0b66','\u0b6f','\u0b71','\u0b71','\u0b82','\u0b83','\u0b85','\u0b8a','\u0b8e','\u0b90','\u0b92','\u0b95',
                '\u0b99','\u0b9a','\u0b9c','\u0b9c','\u0b9e','\u0b9f','\u0ba3','\u0ba4','\u0ba8','\u0baa','\u0bae','\u0bb9',
                '\u0bc0','\u0bc0','\u0bcd','\u0bcd','\u0bd0','\u0bd0','\u0be6','\u0bef','\u0c05','\u0c0c','\u0c0e','\u0c10',
                '\u0c12','\u0c28','\u0c2a','\u0c33','\u0c35','\u0c39','\u0c3d','\u0c40','\u0c46','\u0c48','\u0c4a','\u0c4d',
                '\u0c55','\u0c56','\u0c58','\u0c59','\u0c60','\u0c63','\u0c66','\u0c6f','\u0c85','\u0c8c','\u0c8e','\u0c90',
                '\u0c92','\u0ca8','\u0caa','\u0cb3','\u0cb5','\u0cb9','\u0cbc','\u0cbd','\u0cbf','\u0cbf','\u0cc6','\u0cc6',
                '\u0ccc','\u0ccd','\u0cde','\u0cde','\u0ce0','\u0ce3','\u0ce6','\u0cef','\u0d05','\u0d0c','\u0d0e','\u0d10',
                '\u0d12','\u0d28','\u0d2a','\u0d39','\u0d3d','\u0d3d','\u0d41','\u0d44','\u0d4d','\u0d4d','\u0d60','\u0d63',
                '\u0d66','\u0d6f','\u0d7a','\u0d7f','\u0d85','\u0d96','\u0d9a','\u0db1','\u0db3','\u0dbb','\u0dbd','\u0dbd',
                '\u0dc0','\u0dc6','\u0dca','\u0dca','\u0dd2','\u0dd4','\u0dd6','\u0dd6','\u0e01','\u0e3a','\u0e40','\u0e4e',
                '\u0e50','\u0e59','\u0e81','\u0e82','\u0e84','\u0e84','\u0e87','\u0e88','\u0e8a','\u0e8a','\u0e8d','\u0e8d',
                '\u0e94','\u0e97','\u0e99','\u0e9f','\u0ea1','\u0ea3','\u0ea5','\u0ea5','\u0ea7','\u0ea7','\u0eaa','\u0eab',
                '\u0ead','\u0eb9','\u0ebb','\u0ebd','\u0ec0','\u0ec4','\u0ec6','\u0ec6','\u0ec8','\u0ecd','\u0ed0','\u0ed9',
                '\u0edc','\u0edd','\u0f00','\u0f00','\u0f18','\u0f19','\u0f20','\u0f29','\u0f35','\u0f35','\u0f37','\u0f37',
                '\u0f39','\u0f39','\u0f40','\u0f47','\u0f49','\u0f6c','\u0f71','\u0f7e','\u0f80','\u0f84','\u0f86','\u0f8b',
                '\u0f90','\u0f97','\u0f99','\u0fbc','\u0fc6','\u0fc6','\u1000','\u102a','\u102d','\u1030','\u1032','\u1037',
                '\u1039','\u103a','\u103d','\u1049','\u1050','\u1055','\u1058','\u1061','\u1065','\u1066','\u106e','\u1082',
                '\u1085','\u1086','\u108d','\u108e','\u1090','\u1099','\u10a0','\u10c5','\u10d0','\u10fa','\u10fc','\u10fc',
                '\u1100','\u1159','\u115f','\u11a2','\u11a8','\u11f9','\u1200','\u1248','\u124a','\u124d','\u1250','\u1256',
                '\u1258','\u1258','\u125a','\u125d','\u1260','\u1288','\u128a','\u128d','\u1290','\u12b0','\u12b2','\u12b5',
                '\u12b8','\u12be','\u12c0','\u12c0','\u12c2','\u12c5','\u12c8','\u12d6','\u12d8','\u1310','\u1312','\u1315',
                '\u1318','\u135a','\u135f','\u135f','\u1380','\u138f','\u13a0','\u13f4','\u1401','\u166c','\u166f','\u1676',
                '\u1681','\u169a','\u16a0','\u16ea','\u1700','\u170c','\u170e','\u1714','\u1720','\u1734','\u1740','\u1753',
                '\u1760','\u176c','\u176e','\u1770','\u1772','\u1773','\u1780','\u17b3','\u17b7','\u17bd','\u17c6','\u17c6',
                '\u17c9','\u17d3','\u17d7','\u17d7','\u17dc','\u17dd','\u17e0','\u17e9','\u180b','\u180d','\u1810','\u1819',
                '\u1820','\u1877','\u1880','\u18aa','\u1900','\u191c','\u1920','\u1922','\u1927','\u1928','\u1932','\u1932',
                '\u1939','\u193b','\u1946','\u196d','\u1970','\u1974','\u1980','\u19a9','\u19c1','\u19c7','\u19d0','\u19d9',
                '\u1a00','\u1a18','\u1b00','\u1b03','\u1b05','\u1b34','\u1b36','\u1b3a','\u1b3c','\u1b3c','\u1b42','\u1b42',
                '\u1b45','\u1b4b','\u1b50','\u1b59','\u1b6b','\u1b73','\u1b80','\u1b81','\u1b83','\u1ba0','\u1ba2','\u1ba5',
                '\u1ba8','\u1ba9','\u1bae','\u1bb9','\u1c00','\u1c23','\u1c2c','\u1c33','\u1c36','\u1c37','\u1c40','\u1c49',
                '\u1c4d','\u1c7d','\u1d00','\u1de6','\u1dfe','\u1f15','\u1f18','\u1f1d','\u1f20','\u1f45','\u1f48','\u1f4d',
                '\u1f50','\u1f57','\u1f59','\u1f59','\u1f5b','\u1f5b','\u1f5d','\u1f5d','\u1f5f','\u1f7d','\u1f80','\u1fb4',
                '\u1fb6','\u1fbc','\u1fbe','\u1fbe','\u1fc2','\u1fc4','\u1fc6','\u1fcc','\u1fd0','\u1fd3','\u1fd6','\u1fdb',
                '\u1fe0','\u1fec','\u1ff2','\u1ff4','\u1ff6','\u1ffc','\u203f','\u2040','\u2054','\u2054','\u2071','\u2071',
                '\u207f','\u207f','\u2090','\u2094','\u20d0','\u20dc','\u20e1','\u20e1','\u20e5','\u20f0','\u2102','\u2102',
                '\u2107','\u2107','\u210a','\u2113','\u2115','\u2115','\u2119','\u211d','\u2124','\u2124','\u2126','\u2126',
                '\u2128','\u2128','\u212a','\u212d','\u212f','\u2139','\u213c','\u213f','\u2145','\u2149','\u214e','\u214e',
                '\u2183','\u2184','\u2c00','\u2c2e','\u2c30','\u2c5e','\u2c60','\u2c6f','\u2c71','\u2c7d','\u2c80','\u2ce4',
                '\u2d00','\u2d25','\u2d30','\u2d65','\u2d6f','\u2d6f','\u2d80','\u2d96','\u2da0','\u2da6','\u2da8','\u2dae',
                '\u2db0','\u2db6','\u2db8','\u2dbe','\u2dc0','\u2dc6','\u2dc8','\u2dce','\u2dd0','\u2dd6','\u2dd8','\u2dde',
                '\u2de0','\u2dff','\u2e2f','\u2e2f','\u3005','\u3006','\u302a','\u302f','\u3031','\u3035','\u303b','\u303c',
                '\u3041','\u3096','\u3099','\u309a','\u309d','\u309f','\u30a1','\u30fa','\u30fc','\u30ff','\u3105','\u312d',
                '\u3131','\u318e','\u31a0','\u31b7','\u31f0','\u31ff','\u3400','\u4db5','\u4e00','\u9fc3','\ua000','\ua48c',
                '\ua500','\ua60c','\ua610','\ua62b','\ua640','\ua65f','\ua662','\ua66f','\ua67c','\ua67d','\ua67f','\ua697',
                '\ua717','\ua71f','\ua722','\ua788','\ua78b','\ua78c','\ua7fb','\ua822','\ua825','\ua826','\ua840','\ua873',
                '\ua882','\ua8b3','\ua8c4','\ua8c4','\ua8d0','\ua8d9','\ua900','\ua92d','\ua930','\ua951','\uaa00','\uaa2e',
                '\uaa31','\uaa32','\uaa35','\uaa36','\uaa40','\uaa4c','\uaa50','\uaa59','\uac00','\ud7a3','\uf900','\ufa2d',
                '\ufa30','\ufa6a','\ufa70','\ufad9','\ufb00','\ufb06','\ufb13','\ufb17','\ufb1d','\ufb28','\ufb2a','\ufb36',
                '\ufb38','\ufb3c','\ufb3e','\ufb3e','\ufb40','\ufb41','\ufb43','\ufb44','\ufb46','\ufbb1','\ufbd3','\ufd3d',
                '\ufd50','\ufd8f','\ufd92','\ufdc7','\ufdf0','\ufdfb','\ufe00','\ufe0f','\ufe20','\ufe26','\ufe33','\ufe34',
                '\ufe4d','\ufe4f','\ufe70','\ufe74','\ufe76','\ufefc','\uff10','\uff19','\uff21','\uff3a','\uff3f','\uff3f',
                '\uff41','\uff5a','\uff66','\uffbe','\uffc2','\uffc7','\uffca','\uffcf','\uffd2','\uffd7','\uffda','\uffdc'
			};

		private static readonly char[] escW = new []
			{
				'\u0001', '\u002f', '\u003a', '\u0040', '\u005b', '\u005e', '\u0060', '\u0060', '\u007b', '\u00a9', '\u00ab', '\u00b4',
                '\u00b6','\u00b9','\u00bb','\u00bf','\u00d7','\u00d7','\u00f7','\u00f7','\u02c2','\u02c5','\u02d2','\u02df',
                '\u02e5','\u02eb','\u02ed','\u02ed','\u02ef','\u02ff','\u0375','\u0375','\u0378','\u0379','\u037e','\u0385',
                '\u0387','\u0387','\u038b','\u038b','\u038d','\u038d','\u03a2','\u03a2','\u03f6','\u03f6','\u0482','\u0482',
                '\u0488','\u0489','\u0524','\u0530','\u0557','\u0558','\u055a','\u0560','\u0588','\u0590','\u05be','\u05be',
                '\u05c0','\u05c0','\u05c3','\u05c3','\u05c6','\u05c6','\u05c8','\u05cf','\u05eb','\u05ef','\u05f3','\u060f',
                '\u061b','\u0620','\u065f','\u065f','\u066a','\u066d','\u06d4','\u06d4','\u06dd','\u06de','\u06e9','\u06e9',
                '\u06fd','\u06fe','\u0700','\u070f','\u074b','\u074c','\u07b2','\u07bf','\u07f6','\u07f9','\u07fb','\u0900',
                '\u0903','\u0903','\u093a','\u093b','\u093e','\u0940','\u0949','\u094c','\u094e','\u094f','\u0955','\u0957',
                '\u0964','\u0965','\u0970','\u0970','\u0973','\u097a','\u0980','\u0980','\u0982','\u0984','\u098d','\u098e',
                '\u0991','\u0992','\u09a9','\u09a9','\u09b1','\u09b1','\u09b3','\u09b5','\u09ba','\u09bb','\u09be','\u09c0',
                '\u09c5','\u09cc','\u09cf','\u09db','\u09de','\u09de','\u09e4','\u09e5','\u09f2','\u0a00','\u0a03','\u0a04',
                '\u0a0b','\u0a0e','\u0a11','\u0a12','\u0a29','\u0a29','\u0a31','\u0a31','\u0a34','\u0a34','\u0a37','\u0a37',
                '\u0a3a','\u0a3b','\u0a3d','\u0a40','\u0a43','\u0a46','\u0a49','\u0a4a','\u0a4e','\u0a50','\u0a52','\u0a58',
                '\u0a5d','\u0a5d','\u0a5f','\u0a65','\u0a76','\u0a80','\u0a83','\u0a84','\u0a8e','\u0a8e','\u0a92','\u0a92',
                '\u0aa9','\u0aa9','\u0ab1','\u0ab1','\u0ab4','\u0ab4','\u0aba','\u0abb','\u0abe','\u0ac0','\u0ac6','\u0ac6',
                '\u0ac9','\u0acc','\u0ace','\u0acf','\u0ad1','\u0adf','\u0ae4','\u0ae5','\u0af0','\u0b00','\u0b02','\u0b04',
                '\u0b0d','\u0b0e','\u0b11','\u0b12','\u0b29','\u0b29','\u0b31','\u0b31','\u0b34','\u0b34','\u0b3a','\u0b3b',
                '\u0b3e','\u0b3e','\u0b40','\u0b40','\u0b45','\u0b4c','\u0b4e','\u0b55','\u0b57','\u0b5b','\u0b5e','\u0b5e',
                '\u0b64','\u0b65','\u0b70','\u0b70','\u0b72','\u0b81','\u0b84','\u0b84','\u0b8b','\u0b8d','\u0b91','\u0b91',
                '\u0b96','\u0b98','\u0b9b','\u0b9b','\u0b9d','\u0b9d','\u0ba0','\u0ba2','\u0ba5','\u0ba7','\u0bab','\u0bad',
                '\u0bba','\u0bbf','\u0bc1','\u0bcc','\u0bce','\u0bcf','\u0bd1','\u0be5','\u0bf0','\u0c04','\u0c0d','\u0c0d',
                '\u0c11','\u0c11','\u0c29','\u0c29','\u0c34','\u0c34','\u0c3a','\u0c3c','\u0c41','\u0c45','\u0c49','\u0c49',
                '\u0c4e','\u0c54','\u0c57','\u0c57','\u0c5a','\u0c5f','\u0c64','\u0c65','\u0c70','\u0c84','\u0c8d','\u0c8d',
                '\u0c91','\u0c91','\u0ca9','\u0ca9','\u0cb4','\u0cb4','\u0cba','\u0cbb','\u0cbe','\u0cbe','\u0cc0','\u0cc5',
                '\u0cc7','\u0ccb','\u0cce','\u0cdd','\u0cdf','\u0cdf','\u0ce4','\u0ce5','\u0cf0','\u0d04','\u0d0d','\u0d0d',
                '\u0d11','\u0d11','\u0d29','\u0d29','\u0d3a','\u0d3c','\u0d3e','\u0d40','\u0d45','\u0d4c','\u0d4e','\u0d5f',
                '\u0d64','\u0d65','\u0d70','\u0d79','\u0d80','\u0d84','\u0d97','\u0d99','\u0db2','\u0db2','\u0dbc','\u0dbc',
                '\u0dbe','\u0dbf','\u0dc7','\u0dc9','\u0dcb','\u0dd1','\u0dd5','\u0dd5','\u0dd7','\u0e00','\u0e3b','\u0e3f',
                '\u0e4f','\u0e4f','\u0e5a','\u0e80','\u0e83','\u0e83','\u0e85','\u0e86','\u0e89','\u0e89','\u0e8b','\u0e8c',
                '\u0e8e','\u0e93','\u0e98','\u0e98','\u0ea0','\u0ea0','\u0ea4','\u0ea4','\u0ea6','\u0ea6','\u0ea8','\u0ea9',
                '\u0eac','\u0eac','\u0eba','\u0eba','\u0ebe','\u0ebf','\u0ec5','\u0ec5','\u0ec7','\u0ec7','\u0ece','\u0ecf',
                '\u0eda','\u0edb','\u0ede','\u0eff','\u0f01','\u0f17','\u0f1a','\u0f1f','\u0f2a','\u0f34','\u0f36','\u0f36',
                '\u0f38','\u0f38','\u0f3a','\u0f3f','\u0f48','\u0f48','\u0f6d','\u0f70','\u0f7f','\u0f7f','\u0f85','\u0f85',
                '\u0f8c','\u0f8f','\u0f98','\u0f98','\u0fbd','\u0fc5','\u0fc7','\u0fff','\u102b','\u102c','\u1031','\u1031',
                '\u1038','\u1038','\u103b','\u103c','\u104a','\u104f','\u1056','\u1057','\u1062','\u1064','\u1067','\u106d',
                '\u1083','\u1084','\u1087','\u108c','\u108f','\u108f','\u109a','\u109f','\u10c6','\u10cf','\u10fb','\u10fb',
                '\u10fd','\u10ff','\u115a','\u115e','\u11a3','\u11a7','\u11fa','\u11ff','\u1249','\u1249','\u124e','\u124f',
                '\u1257','\u1257','\u1259','\u1259','\u125e','\u125f','\u1289','\u1289','\u128e','\u128f','\u12b1','\u12b1',
                '\u12b6','\u12b7','\u12bf','\u12bf','\u12c1','\u12c1','\u12c6','\u12c7','\u12d7','\u12d7','\u1311','\u1311',
                '\u1316','\u1317','\u135b','\u135e','\u1360','\u137f','\u1390','\u139f','\u13f5','\u1400','\u166d','\u166e',
                '\u1677','\u1680','\u169b','\u169f','\u16eb','\u16ff','\u170d','\u170d','\u1715','\u171f','\u1735','\u173f',
                '\u1754','\u175f','\u176d','\u176d','\u1771','\u1771','\u1774','\u177f','\u17b4','\u17b6','\u17be','\u17c5',
                '\u17c7','\u17c8','\u17d4','\u17d6','\u17d8','\u17db','\u17de','\u17df','\u17ea','\u180a','\u180e','\u180f',
                '\u181a','\u181f','\u1878','\u187f','\u18ab','\u18ff','\u191d','\u191f','\u1923','\u1926','\u1929','\u1931',
                '\u1933','\u1938','\u193c','\u1945','\u196e','\u196f','\u1975','\u197f','\u19aa','\u19c0','\u19c8','\u19cf',
                '\u19da','\u19ff','\u1a19','\u1aff','\u1b04','\u1b04','\u1b35','\u1b35','\u1b3b','\u1b3b','\u1b3d','\u1b41',
                '\u1b43','\u1b44','\u1b4c','\u1b4f','\u1b5a','\u1b6a','\u1b74','\u1b7f','\u1b82','\u1b82','\u1ba1','\u1ba1',
                '\u1ba6','\u1ba7','\u1baa','\u1bad','\u1bba','\u1bff','\u1c24','\u1c2b','\u1c34','\u1c35','\u1c38','\u1c3f',
                '\u1c4a','\u1c4c','\u1c7e','\u1cff','\u1de7','\u1dfd','\u1f16','\u1f17','\u1f1e','\u1f1f','\u1f46','\u1f47',
                '\u1f4e','\u1f4f','\u1f58','\u1f58','\u1f5a','\u1f5a','\u1f5c','\u1f5c','\u1f5e','\u1f5e','\u1f7e','\u1f7f',
                '\u1fb5','\u1fb5','\u1fbd','\u1fbd','\u1fbf','\u1fc1','\u1fc5','\u1fc5','\u1fcd','\u1fcf','\u1fd4','\u1fd5',
                '\u1fdc','\u1fdf','\u1fed','\u1ff1','\u1ff5','\u1ff5','\u1ffd','\u203e','\u2041','\u2053','\u2055','\u2070',
                '\u2072','\u207e','\u2080','\u208f','\u2095','\u20cf','\u20dd','\u20e0','\u20e2','\u20e4','\u20f1','\u2101',
                '\u2103','\u2106','\u2108','\u2109','\u2114','\u2114','\u2116','\u2118','\u211e','\u2123','\u2125','\u2125',
                '\u2127','\u2127','\u2129','\u2129','\u212e','\u212e','\u213a','\u213b','\u2140','\u2144','\u214a','\u214d',
                '\u214f','\u2182','\u2185','\u2bff','\u2c2f','\u2c2f','\u2c5f','\u2c5f','\u2c70','\u2c70','\u2c7e','\u2c7f',
                '\u2ce5','\u2cff','\u2d26','\u2d2f','\u2d66','\u2d6e','\u2d70','\u2d7f','\u2d97','\u2d9f','\u2da7','\u2da7',
                '\u2daf','\u2daf','\u2db7','\u2db7','\u2dbf','\u2dbf','\u2dc7','\u2dc7','\u2dcf','\u2dcf','\u2dd7','\u2dd7',
                '\u2ddf','\u2ddf','\u2e00','\u2e2e','\u2e30','\u3004','\u3007','\u3029','\u3030','\u3030','\u3036','\u303a',
                '\u303d','\u3040','\u3097','\u3098','\u309b','\u309c','\u30a0','\u30a0','\u30fb','\u30fb','\u3100','\u3104',
                '\u312e','\u3130','\u318f','\u319f','\u31b8','\u31ef','\u3200','\u33ff','\u4db6','\u4dff','\u9fc4','\u9fff',
                '\ua48d','\ua4ff','\ua60d','\ua60f','\ua62c','\ua63f','\ua660','\ua661','\ua670','\ua67b','\ua67e','\ua67e',
                '\ua698','\ua716','\ua720','\ua721','\ua789','\ua78a','\ua78d','\ua7fa','\ua823','\ua824','\ua827','\ua83f',
                '\ua874','\ua881','\ua8b4','\ua8c3','\ua8c5','\ua8cf','\ua8da','\ua8ff','\ua92e','\ua92f','\ua952','\ua9ff',
                '\uaa2f','\uaa30','\uaa33','\uaa34','\uaa37','\uaa3f','\uaa4d','\uaa4f','\uaa5a','\uabff','\ud7a4','\uf8ff',
                '\ufa2e','\ufa2f','\ufa6b','\ufa6f','\ufada','\ufaff','\ufb07','\ufb12','\ufb18','\ufb1c','\ufb29','\ufb29',
                '\ufb37','\ufb37','\ufb3d','\ufb3d','\ufb3f','\ufb3f','\ufb42','\ufb42','\ufb45','\ufb45','\ufbb2','\ufbd2',
                '\ufd3e','\ufd4f','\ufd90','\ufd91','\ufdc8','\ufdef','\ufdfc','\ufdff','\ufe10','\ufe1f','\ufe27','\ufe32',
                '\ufe35','\ufe4c','\ufe50','\ufe6f','\ufe75','\ufe75','\ufefd','\uff0f','\uff1a','\uff20','\uff3b','\uff3e',
                '\uff40','\uff40','\uff5b','\uff65','\uffbf','\uffc1','\uffc8','\uffc9','\uffd0','\uffd1','\uffd8','\uffd9'
			};

        private CharSet EscapedCharToAcceptCharRange(char c)
        {
            switch (c)
            {
                // A lot of these are REALLY funky numbers. Tibetan numbers and such. You name it
                case 'd':
                    return new CharSet(false, escd);
                // Shorthand for [^0-9]
                case 'D':
					return new CharSet(false, escD);
                case 's':
                    return AllWhitespaceCharacters;
                case 'S':
                    return AllCharactersExceptNull.Except(AllWhitespaceCharacters);
                case 'w':
                    return new CharSet(false, escw);
                case 'W':
					return new CharSet(false, escW);
                case 'n':
                    return SingleChar('\n');
                case 'r':
                    return SingleChar('\r');
                case 't':
                    return SingleChar('\t');
                case '.':
                case '*':
                case '|':
                case '[':
                case ']':
                case '+':
                case '(':
                case ')':
                case '\\':
                case '{':
                case '}':
                case ' ':
                case '?':
                    return SingleChar(c);
                default:
                    return new CharSet();   // Empty charset, might be added to
            }
        }

        private CharSet SingleChar(char c)
        {
            var cs = new CharSet();
            cs.Add(c);
            return cs;
        }

        private CharSet EscapedCharToAcceptCharsInClass(char c)
        {
            // There are some additional escapeable characters for a character class
            switch (c)
            {
                case '-':
                case '^':
                    return SingleChar(c);

            }
            return EscapedCharToAcceptCharRange(c);
        }

        public RegExToken NextToken()
        {
            // These keeps track of classes
            var classState = new CharacterClassState();
            var numberedRepetitionState = new NumberedRepetitionState();
            state = State.Normal;

            while (input.Peek() != -1)
            {
                var c = (char)input.Read();
                
                switch (state)
                {
                    case State.Normal:
                        switch (c)
                        {
                            case '\\':
                                state = State.NormalEscaped;
                                break;
                            case '[':
                                state = State.BeginCharacterClass;
                                break;
                            case '{':
                                state = State.NumberedRepetition;
                                break;

                            case '(':   return new RegExToken { Type = RegExToken.TokenType.OperatorOpenParanthesis };
                            case ')':   return new RegExToken { Type = RegExToken.TokenType.OperatorCloseParanthesis };
                            case '|':   return new RegExToken { Type = RegExToken.TokenType.OperatorOr };
                            case '+':   return new RegExToken { Type = RegExToken.TokenType.OperatorPlus };
                            case '*':   return new RegExToken { Type = RegExToken.TokenType.OperatorMul };
                            case '?':   return new RegExToken { Type = RegExToken.TokenType.OperatorQuestion };
                            case '.':   return new RegExToken { Type = RegExToken.TokenType.Accept, Characters = AllCharactersExceptNull };
                            default:    return new RegExToken { Type = RegExToken.TokenType.Accept, Characters = SingleChar(c)};
                        }
                        break;

                    case State.NormalEscaped:
                        {
                            var characters = EscapedCharToAcceptCharRange(c);
                            if (!characters.Any())
                            {
                                throw new LexerConstructionException(string.Format("Unknown escaped character '{0}'", c));
                            }
                            return new RegExToken {Characters = characters, Type = RegExToken.TokenType.Accept};
                        }

                    case State.BeginCharacterClass:
                        switch (c)
                        {
                            case '^':
                                if (classState.Negated)
                                {
                                    // If the classstate is ALREADY negated
                                    // Readd the ^ to the expression
                                    classState.LastChar = '^';
                                    state = State.InsideCharacterClass;
                                }
                                classState.Negated = true;
                                break;
                            case '[':
                            case ']':
                            case '-':
                                // This does not break the character class TODO: I THINK!!!
                                classState.LastChar = c;
                                break;
                            case '\\':
                                state = State.InsideCharacterClassEscaped;
                                break;
                            default:
                                classState.LastChar = c;
                                state = State.InsideCharacterClass;
                                break;
                        }
                        break;

                    case State.InsideCharacterClass:
                        switch (c)
                        {
                            case '-':
                                state = State.RangeEnd;
                                break;
                            case '[':
                                throw new LexerConstructionException("Opening new character class inside an already open one");
                            case ']':
                                if (classState.LastChar != (char)0) 
                                    classState.CharsSet.Add(classState.LastChar);

                                // Ending class
                                return new RegExToken
                                           {
                                               Type = RegExToken.TokenType.Accept,
                                               Characters = classState.Negated
                                                                ? AllCharactersExceptNull.Except(classState.CharsSet)
                                                                : classState.CharsSet
                                           };
                            case '\\':
                                state = State.InsideCharacterClassEscaped;
                                break;
                            default:
                                if (classState.LastChar != 0)
                                    classState.CharsSet.Add(classState.LastChar);
                                classState.LastChar = c;
                                break;
                        }
                        break;

                    case State.InsideCharacterClassEscaped:
                        {
                            var characters = EscapedCharToAcceptCharsInClass(c);
                            if (!characters.Any())
                            {
                                throw new LexerConstructionException(string.Format("Unknown escaped character '{0}' in character class", c));
                            }

                            if (classState.LastChar != 0)
                                classState.CharsSet.Add(classState.LastChar);

                            classState.CharsSet.UnionWith(characters);
                            classState.LastChar = (char)0;
                            state = State.InsideCharacterClass;
                        }
                        break;


                    case State.RangeEnd:
                        switch (c)
                        {
                            case ']':
                                // We found the - at the position BEFORE the end of the class
                                // which means we should handle it as a litteral and end the class
                                classState.CharsSet.Add(classState.LastChar);
                                classState.CharsSet.Add('-');

                                return new RegExToken
                                {
                                    Type = RegExToken.TokenType.Accept,
                                    Characters = classState.Negated
                                                     ? AllCharactersExceptNull.Except(classState.CharsSet)
                                                     : classState.CharsSet
                                };

                            default:
                                char lastClassChar = classState.LastChar;
                                char from = lastClassChar < c ? lastClassChar : c;
                                char to = lastClassChar < c ? c : lastClassChar;
                                classState.CharsSet.AddRange(from, to);
                                classState.LastChar = (char) 0;
                                state = State.InsideCharacterClass;
                                break;
                        }
                        break;

                    case State.NumberedRepetition:
                        switch (c)
                        {
                            case '0':   // Is it really OK to start with a 0. It is now.
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                numberedRepetitionState.Chars.Add(c);
                                break;
                            case '}':
                            case ':':
                            case ',':
                                // Parse whatever is in Chars
                                int reps;

                                // Number is required in FIRST part but OPTIONAL in the second
                                if (numberedRepetitionState.Chars.Any() || numberedRepetitionState.CurrentPart == 0)
                                {
                                    if (!int.TryParse(new string(numberedRepetitionState.Chars.ToArray()), out reps))
                                    {
                                        throw new LexerConstructionException("Numbered repetition operator contains operand that is not a number");
                                    }
                                }
                                else
                                {
                                    // End up here when nothing specified in the last part.
                                    // Use the max value to say that it can be infinite numbers.
                                    reps = int.MaxValue;
                                }
                                numberedRepetitionState.Chars.Clear();

                                // Set the right value
                                if (numberedRepetitionState.CurrentPart == 0)
                                {
                                    numberedRepetitionState.MinRepetitions = reps;
                                }
                                else
                                {
                                    numberedRepetitionState.MaxRepetitions = reps;
                                }

                                if (c == ':' || c == ',')
                                {
                                    ++numberedRepetitionState.CurrentPart;
                                    if (numberedRepetitionState.CurrentPart > 1)
                                        throw new LexerConstructionException("More than one , in numbered repetition.");
                                }
                                else
                                {
                                    return new RegExToken
                                    {
                                        Type = RegExToken.TokenType.NumberedRepeat,
                                        MinRepetitions = numberedRepetitionState.MinRepetitions, 
                                        MaxRepetitions = numberedRepetitionState.MaxRepetitions
                                    };
                                }
                                break;
                            default:
                                throw new LexerConstructionException(
                                    string.Format("Illegal character {0} in numbered repetition", c));
                        }
                        break;
                }
            }

            // We get here if we try to lex when the expression has ended.
            return null;
        }

        private static CharSet CharRange(char start, char end)
        {
            var charRange = new CharSet();
            charRange.AddRange(start, end);
            return charRange;
        }

        protected static CharSet AllCharactersExceptNull
        {
            get
            {
                return CharRange((char) 1, char.MaxValue);                
            }
        }

        protected static CharSet AllWhitespaceCharacters
        {
            get
            {
                return new CharSet(false, '\u0009', '\u000d', '\u0020', '\u0020', '\u0085', '\u0085', '\u00a0', '\u00a0', '\u1680', '\u1680', '\u180e', '\u180e',
                                   '\u2000', '\u200a', '\u2028', '\u2029', '\u202f', '\u202f', '\u205f', '\u205f', '\u3000', '\u3000');
            }
        }
    }
}
