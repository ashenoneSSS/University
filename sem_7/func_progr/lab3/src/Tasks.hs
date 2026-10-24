module Tasks
  ( alphabeticalWords
  , groupByInitial
  , formatParagraphs
  ) where

import Alphabet
import Data.Char (toLower, toUpper)
import Data.List (groupBy, nubBy, sortOn)
import Data.Function (on)
import Prelude hiding (Word)
import TextModel

alphabeticalWords :: Text -> [String]
alphabeticalWords =
  sortOn wordKey . nubBy ((==) `on` map toLower) . map (map toLower . wordString) . wordsOf

groupByInitial :: [String] -> [(Char, [String])]
groupByInitial ws = [(toUpper (initial (head g)), g) | g <- groupBy sameInitial ws]

formatParagraphs :: Int -> [(Char, [String])] -> [String]
formatParagraphs width = concatMap paragraph
  where
    indent = "    "
    paragraph (letter, ws) = wrap (indent ++ [letter] ++ ":") ws
    wrap line [] = [line]
    wrap line (w : rest)
      | length line + 1 + length w > width = line : wrap w rest
      | otherwise = wrap (line ++ " " ++ w) rest
