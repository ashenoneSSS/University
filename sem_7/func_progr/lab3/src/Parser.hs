module Parser
  ( normalizeSpaces
  , tokenize
  , parseText
  ) where

import Data.Char (isAlpha, isDigit, isSpace)
import Prelude hiding (Word)
import TextModel

normalizeSpaces :: String -> String
normalizeSpaces = unwords . words

isJoiner :: Char -> Bool
isJoiner c = c `elem` "'’ʼ-"

isTerminator :: Char -> Bool
isTerminator c = c `elem` ".!?…"

tokenize :: String -> [Token]
tokenize [] = []
tokenize s@(c : rest)
  | isSpace c = tokenize rest
  | isAlpha c =
      let (body, remaining) = spanWord s
       in WordToken (Word (map toSymbol body)) : tokenize remaining
  | isDigit c =
      let (digits, remaining) = span isDigit s
       in NumberToken digits : tokenize remaining
  | otherwise = PunctToken (Punctuation c) : tokenize rest
  where
    toSymbol ch
      | isAlpha ch = Letter ch
      | isDigit ch = Digit ch
      | otherwise = Joiner ch

spanWord :: String -> (String, String)
spanWord (x : y : rest)
  | isJoiner x && isAlpha y = let (w, r) = spanWord (y : rest) in (x : w, r)
spanWord (x : rest)
  | isAlpha x || isDigit x = let (w, r) = spanWord rest in (x : w, r)
spanWord rest = ([], rest)

splitSentences :: [Token] -> [Sentence]
splitSentences [] = []
splitSentences tokens =
  let (body, rest) = break isEnd tokens
      (ends, remaining) = span isEnd rest
   in Sentence (body ++ ends) : splitSentences remaining
  where
    isEnd (PunctToken (Punctuation c)) = isTerminator c
    isEnd _ = False

parseText :: String -> Text
parseText = Text . splitSentences . tokenize . normalizeSpaces
