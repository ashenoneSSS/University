module TextModel
  ( Symbol (..)
  , Word (..)
  , Punctuation (..)
  , Token (..)
  , Sentence (..)
  , Text (..)
  , symbolChar
  , wordString
  , wordsOf
  , sentenceWords
  ) where

import Prelude hiding (Word)

data Symbol
  = Letter Char
  | Digit Char
  | Joiner Char
  deriving (Eq, Show)

newtype Word = Word [Symbol]
  deriving (Eq, Show)

newtype Punctuation = Punctuation Char
  deriving (Eq, Show)

data Token
  = WordToken Word
  | NumberToken String
  | PunctToken Punctuation
  deriving (Eq, Show)

newtype Sentence = Sentence [Token]
  deriving (Eq, Show)

newtype Text = Text [Sentence]
  deriving (Eq, Show)

symbolChar :: Symbol -> Char
symbolChar (Letter c) = c
symbolChar (Digit c)  = c
symbolChar (Joiner c) = c

wordString :: Word -> String
wordString (Word symbols) = map symbolChar symbols

sentenceWords :: Sentence -> [Word]
sentenceWords (Sentence tokens) = [w | WordToken w <- tokens]

wordsOf :: Text -> [Word]
wordsOf (Text sentences) = concatMap sentenceWords sentences
