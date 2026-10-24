module Alphabet
  ( letterKey
  , wordKey
  , sameInitial
  , initial
  ) where

import Data.Char (isAlpha, ord, toLower)
import Data.Function (on)
import Data.List (elemIndex)

latin, ukrainian :: String
latin = ['a' .. 'z']
ukrainian = "абвгґдеєжзиіїйклмнопрстуфхцчшщьюя"

letterKey :: Char -> Int
letterKey ch =
  case (elemIndex c ukrainian, elemIndex c latin) of
    (Just i, _) -> 100 + i
    (_, Just i) -> i
    _ -> 1000 + ord c
  where
    c = toLower ch

wordKey :: String -> [Int]
wordKey = map letterKey . filter isAlpha

initial :: String -> Char
initial = toLower . head

sameInitial :: String -> String -> Bool
sameInitial = (==) `on` (letterKey . initial)
