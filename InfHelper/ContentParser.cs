﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfHelper.Exceptions;
using InfHelper.Models;
using InfHelper.Models.Tokens;

namespace InfHelper.Parsers
{
    public class ContentParser
    {
        private Category currentCategory;
        private Key currentKey;
        private readonly ITokenParser parser;
        private string keyTmpValue;
        private Action previousParsing;
        private string parsingType = "";

        public ContentParser() : this(new BasicTokenParser())
        {

        }

        public ContentParser(ITokenParser parser)
        {
            this.parser = parser;
            parser.InvalidTokenFound += InvalidTokenFound;
        }

        public void Parse(string content)
        {
            InitMainParsing();
            parser.Parse(content);
            ValueParsingComplete();
            KeyParsingComplete();
            CategoryParsingComplete();
        }

        /// <summary>
        /// When category parsing is completed
        /// </summary>
        public event EventHandler<Category> CategoryDiscovered;

        /// <summary>
        /// Inits main parsing state. Skips inlines comments, white spaces and new lines and init new category parsing when category opening tokenBase found.
        /// </summary>
        protected void InitMainParsing()
        {
            parsingType = "main parsing";
            ClearAllMyCallbacks();
            parser.ValidTokenFound += ValidTokenFoundDuringMainParsing;

            parser.AllowedTokens = new HashSet<TokenBase>
            {
                new InlineCommentToken(),
                new CategoryOpeningToken(),
            };

            parser.IgnoredTokens = new HashSet<TokenBase>
            {
                new WhiteSpaceToken(),
                new NewLineToken()
            };
        }

        /// <summary>
        /// Parse only letter tokens, end parsing when closing tokenBase found.
        /// </summary>
        protected void InitCategoryParsing()
        {
            parsingType = "category parsing";
            currentCategory = new Category();
            ClearAllMyCallbacks();
            parser.ValidTokenFound += ValidTokenFoundDuringCategoryParsing;
            parser.IgnoredTokens?.Clear();
            parser.AllowedTokens = new HashSet<TokenBase>
            {
                new CategoryClosingToken(),
                new LetterToken()
            };
        }

        /// <summary>
        /// Parse key id.
        /// </summary>
        protected void InitKeyIdParsing()
        {
            parsingType = "key id parsing";
            currentKey = new Key();
            ClearAllMyCallbacks();
            parser.ValidTokenFound += ValidTokenFoundDuringKeyIdParsing;

            parser.AllowedTokens = new HashSet<TokenBase>
            {
                new InlineCommentToken(),
                new LetterToken(),
                new EqualityToken(),
                new WhiteSpaceToken(),
                new CategoryOpeningToken(),
                new NewLineToken(),
                new ValueSeparatorToken(),
                new ValueMarkerToken(),
            };

            parser.IgnoredTokens = new HashSet<TokenBase>
            {
                new LineConcatenatorToken(),
            };
        }

        /// <summary>
        /// Parse values for current key
        /// </summary>
        protected void InitKeyValueParsing()
        {
            parsingType = "key value parsing";
            ClearAllMyCallbacks();
            parser.ValidTokenFound += ValidTokenFoundDuringKeyValueParsing;

            parser.AllowedTokens = new HashSet<TokenBase>
            {
                new ValueSeparatorToken(),
                new LetterToken(),
                new NewLineToken(),
                new WhiteSpaceToken(),
                new ValueMarkerToken()
            };

            parser.IgnoredTokens = new HashSet<TokenBase>()
            {
                new LineConcatenatorToken(),
                new InlineCommentToken(),
                new EqualityToken()
            };
        }

        protected void InitPureValueParsing()
        {
            parsingType = "pure value parsing";
            ClearAllMyCallbacks();
            parser.ValidTokenFound += ValidTokenFoundDuringPureValueParsing;

            parser.AllowedTokens = new HashSet<TokenBase>
            {
                new LetterToken(),
                new ValueMarkerToken(),
                new WhiteSpaceToken(),
                new ValueSeparatorToken()
            };

            parser.IgnoredTokens = new HashSet<TokenBase>()
            {
                new LineConcatenatorToken(),
                new InlineCommentToken(),
                new EqualityToken()
            };
        }

        /// <summary>
        /// Parse comments
        /// </summary>
        protected void InitCommentParsing(Action previous)
        {
            parsingType = "comment parsing";
            previousParsing = previous;
            ClearAllMyCallbacks();
            parser.ValidTokenFound += ValidTokenFoundDuringCommentParsing;

            parser.AllowedTokens = new HashSet<TokenBase>
            {
                new NewLineToken(),
            };

            parser.IgnoredTokens = new HashSet<TokenBase>()
            {
                new LetterToken(),
                new WhiteSpaceToken(),
                new ValueMarkerToken(),
                new ValueSeparatorToken(),
                new LineConcatenatorToken(),
                new EqualityToken(),
                new InlineCommentToken(),
                new CategoryOpeningToken(),
                new CategoryClosingToken(),
            };
        }

        // Parsing value inside ""
        private void ValidTokenFoundDuringPureValueParsing(object sender, TokenBase tokenBase)
        {
            switch (tokenBase.Type)
            {
                case TokenType.Letter:
                case TokenType.ValueSeparator:
                case TokenType.WhiteSpace:
                    keyTmpValue += tokenBase.Symbol;
                    break;
                case TokenType.ValueMarker:
                    ValueParsingComplete(true);
                    InitKeyValueParsing();
                    break;
                default:
                    throw new InvalidTokenException("Invalid tokenBase found during comment parsing: " + tokenBase.Symbol);
            }
        }

        //Parsing inline comment
        private void ValidTokenFoundDuringCommentParsing(object sender, TokenBase tokenBase)
        {
            switch (tokenBase.Type)
            {
                case TokenType.NewLine:
                    previousParsing();
                    break;
                default:
                    throw new InvalidTokenException("Invalid tokenBase found during comment parsing: " + tokenBase.Symbol);
            }
        }

        //Parsing top layer
        protected void ValidTokenFoundDuringMainParsing(object sender, TokenBase tokenBase)
        {
            switch (tokenBase.Type)
            {
                case TokenType.InlineComment:
                    //go to next line, init comment parsing
                    InitCommentParsing(InitMainParsing);
                    break;
                case TokenType.CategoryOpening:
                    currentCategory = new Category();
                    InitCategoryParsing();
                    break;
                default:
                    throw new InvalidTokenException("Invalid tokenBase found during parsing of the file: " + tokenBase.Symbol);
            }
        }

        //when parsing a category
        protected void ValidTokenFoundDuringCategoryParsing(object sender, TokenBase tokenBase)
        {
            switch (tokenBase.Type)
            {
                case TokenType.CategoryClosing:
                    InitKeyIdParsing();
                    break;
                case TokenType.Letter:
                    currentCategory.Name += tokenBase.Symbol;
                    break;
                default:
                    throw new InvalidTokenException("Invalid tokenBase found during parsing of the file: " + tokenBase.Symbol);
            }
        }

        //when parsing a tokenBase id
        protected void ValidTokenFoundDuringKeyIdParsing(object sender, TokenBase tokenBase)
        {
            switch (tokenBase.Type)
            {
                case TokenType.ValueMarker:
                    InitPureValueParsing();
                    break;
                case TokenType.ValueSeparator:
                    SerializeCurrentTmpValueAsAnonymousKey();
                    break;
                case TokenType.NewLine:
                    if (!string.IsNullOrEmpty(keyTmpValue))
                        SerializeCurrentTmpValueAsAnonymousKey();
                    break;
                case TokenType.EQ:
                    // multiple EQ tokens in formula
                    if (!string.IsNullOrEmpty(currentKey.Id))
                    {
                        throw new InvalidTokenException("Equality tokenBase detected, but not expected.");
                    }
                    KeyIdParsingCompleted();
                    InitKeyValueParsing();
                    break;
                case TokenType.WhiteSpace:
                    //ignore spaces at the begining
                    if (!string.IsNullOrEmpty(keyTmpValue))
                    {
                        keyTmpValue += tokenBase.Symbol;
                    }
                    break;
                case TokenType.Letter:
                    keyTmpValue += tokenBase.Symbol;
                    break;
                case TokenType.CategoryOpening:
                    KeyParsingComplete();
                    CategoryParsingComplete();
                    InitCategoryParsing();
                    break;
                case TokenType.InlineComment:
                    InitCommentParsing(InitKeyIdParsing);
                    break;
                default:
                    throw new InvalidTokenException("Invalid tokenBase found during parsing of the file: " + tokenBase.Symbol);
            }
        }

        //When parsing value
        protected void ValidTokenFoundDuringKeyValueParsing(object sender, TokenBase tokenBase)
        {
            switch (tokenBase.Type)
            {
                case TokenType.ValueSeparator:
                    ValueParsingComplete();
                    break;
                case TokenType.Letter:
                    keyTmpValue += tokenBase.Symbol;
                    break;
                case TokenType.NewLine:
                    ValueParsingComplete();
                    KeyParsingComplete();
                    InitKeyIdParsing();
                    break;
                case TokenType.WhiteSpace:
                    if (string.IsNullOrEmpty(keyTmpValue))
                    {
                        ValueParsingComplete();
                    }
                    break;
                case TokenType.ValueMarker:
                    InitPureValueParsing();
                    break;
            }
        }

        protected void InvalidTokenFound(object sender, TokenBase tokenBase)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Invalid tokenBase found during {parsingType} parsing: ");
            builder.AppendLine($"Symbol: {tokenBase.Symbol}");
            builder.AppendLine($"Token type: {tokenBase.Type}");
            builder.AppendLine($"Allowed tokens: {string.Join(", ", parser.AllowedTokens.Select(t => t.Type.ToString()))}");
            builder.AppendLine($"Ignored tokens: {string.Join(", ", parser.IgnoredTokens.Select(t => t.Type.ToString()))}");
            throw new InvalidTokenException(builder.ToString());
        }

        protected void KeyParsingComplete()
        {
            if (currentKey != null && currentKey.KeyValues.Any())
            {
                currentCategory.Keys.Add(currentKey);
                currentKey = null;
            }
        }

        protected void ValueParsingComplete(bool pure = false)
        {
            if (!string.IsNullOrEmpty(keyTmpValue))
            {
                KeyValue keyValue;
                if (!pure)
                {
                    keyValue = new KeyValue
                    {
                        Value = keyTmpValue
                    };
                }
                else
                {
                    keyValue = new PureValue
                    {
                        Value = keyTmpValue
                    };
                }
                currentKey.KeyValues.Add(keyValue);
                keyTmpValue = null;
            }
        }

        protected void CategoryParsingComplete()
        {
            if (currentCategory != null)
            {
                CategoryDiscovered?.Invoke(this, currentCategory);
                currentCategory = null;
            }
        }

        protected void SerializeCurrentTmpValueAsAnonymousKey()
        {
            //TODO Implement this
            var keyValue = new KeyValue
            {
                Value = keyTmpValue
            };
            currentKey.KeyValues.Add(keyValue);
            keyTmpValue = null;
        }

        protected void KeyIdParsingCompleted()
        {
            //check for trailing white spaces
            for (var i = keyTmpValue.Length - 1; i >= 0; i--)
            {
                if (char.IsWhiteSpace(keyTmpValue[i]))
                {
                    keyTmpValue = keyTmpValue.Remove(i, 1);
                }
            }

            currentKey.Id = keyTmpValue;
            keyTmpValue = null;
        }

        private void ClearAllMyCallbacks()
        {
            parser.ValidTokenFound -= ValidTokenFoundDuringMainParsing;
            parser.ValidTokenFound -= ValidTokenFoundDuringCategoryParsing;
            parser.ValidTokenFound -= ValidTokenFoundDuringKeyIdParsing;
            parser.ValidTokenFound -= ValidTokenFoundDuringKeyValueParsing;
            parser.ValidTokenFound -= ValidTokenFoundDuringCommentParsing;
            parser.ValidTokenFound -= ValidTokenFoundDuringPureValueParsing;
        }
    }
}