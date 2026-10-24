module Pretty
  ( Pretty (..)
  , printAll
  ) where

import Data.Time (Day)
import Types

class Pretty a where
  pretty :: a -> String

instance Pretty Category where
  pretty c = "[" ++ show (categoryId c) ++ "] " ++ categoryName c

instance Pretty Author where
  pretty a =
    "[" ++ show (authorId a) ++ "] " ++ authorName a ++ " (" ++ authorOrganization a ++ ")"
      ++ maybe "" (", " ++) (authorEmail a)

instance Pretty Software where
  pretty s =
    unlines
      [ "[" ++ show (softwareId s) ++ "] " ++ softwareName s ++ " v" ++ softwareVersion s
      , "    Анотація:   " ++ softwareAnnotation s
      , "    Дистрибутив: " ++ softwarePath s
      , "    Відкрито:   " ++ showDay (softwareOpenedAt s)
          ++ ", діє до: " ++ maybe "безстроково" showDay (softwareValidUntil s)
      , "    Умови:      " ++ softwareConditions s
      ]

instance Pretty User where
  pretty u =
    "[" ++ show (userId u) ++ "] " ++ userName u ++ " — " ++ userRole u ++ ", " ++ userUnit u
      ++ maybe "" (", " ++) (userEmail u)

instance Pretty Usage where
  pretty u =
    "[" ++ show (usageId u) ++ "] ПЗ #" ++ show (usageSoftwareId u) ++ ", користувач #"
      ++ show (usageUserId u) ++ ", " ++ showDay (usageDate u) ++ ", " ++ show (usageMinutes u) ++ " хв"

instance Pretty Review where
  pretty r =
    "[" ++ show (reviewId r) ++ "] ПЗ #" ++ show (reviewSoftwareId r) ++ ", оцінка "
      ++ replicate (reviewRating r) '*' ++ " — " ++ reviewComment r

showDay :: Day -> String
showDay = show

printAll :: Pretty a => [a] -> IO ()
printAll [] = putStrLn "  (немає записів)"
printAll xs = mapM_ (putStrLn . pretty) xs
