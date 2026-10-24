module Editable
  ( Editable (..)
  ) where

import Data.Time (getCurrentTime, utctDay)
import Database.MySQL.Simple (Connection)
import Entity (fetchAll)
import Input
import Pretty (printAll)
import Types

class Editable a where
  title  :: a -> String
  create :: Connection -> IO a
  edit   :: Connection -> a -> IO a

instance Editable Category where
  title _ = "Вид програмного засобу"
  create _ = Category 0 <$> ask "Назва виду"
  edit _ c = do
    n <- askDefault "Назва виду" (categoryName c)
    pure c { categoryName = n }

instance Editable Author where
  title _ = "Автор"
  create _ = Author 0 <$> ask "ПІБ / назва автора" <*> ask "Організація" <*> askMaybe "E-mail" Nothing
  edit _ a = do
    n <- askDefault "ПІБ / назва автора" (authorName a)
    o <- askDefault "Організація" (authorOrganization a)
    e <- askMaybe "E-mail" (authorEmail a)
    pure a { authorName = n, authorOrganization = o, authorEmail = e }

instance Editable User where
  title _ = "Користувач"
  create _ =
    User 0 <$> ask "ПІБ" <*> ask "Роль (студент/викладач/співробітник)" <*> ask "Група / підрозділ"
           <*> askMaybe "E-mail" Nothing
  edit _ u = do
    n <- askDefault "ПІБ" (userName u)
    r <- askDefault "Роль" (userRole u)
    g <- askDefault "Група / підрозділ" (userUnit u)
    e <- askMaybe "E-mail" (userEmail u)
    pure u { userName = n, userRole = r, userUnit = g, userEmail = e }

instance Editable Software where
  title _ = "Програмний засіб"
  create conn = do
    putStrLn "Види програмних засобів:"
    printAll =<< (fetchAll conn :: IO [Category])
    Software 0
      <$> ask "Назва"
      <*> ask "Анотація"
      <*> askInt "Код виду"
      <*> ask "Версія"
      <*> ask "Де записаний дистрибутив"
      <*> askDay "Дата відкриття для використання"
      <*> askMaybeDay "Діє до" Nothing
      <*> ask "Умови використання"
  edit conn s = do
    putStrLn "Види програмних засобів:"
    printAll =<< (fetchAll conn :: IO [Category])
    n  <- askDefault "Назва" (softwareName s)
    a  <- askDefault "Анотація" (softwareAnnotation s)
    c  <- askIntDefault "Код виду" (softwareCategoryId s)
    v  <- askDefault "Версія" (softwareVersion s)
    p  <- askDefault "Дистрибутив" (softwarePath s)
    o  <- askDayDefault "Дата відкриття" (softwareOpenedAt s)
    u  <- askMaybeDay "Діє до" (softwareValidUntil s)
    cn <- askDefault "Умови використання" (softwareConditions s)
    pure s { softwareName = n, softwareAnnotation = a, softwareCategoryId = c, softwareVersion = v
           , softwarePath = p, softwareOpenedAt = o, softwareValidUntil = u, softwareConditions = cn }

instance Editable Usage where
  title _ = "Факт використання"
  create _ = do
    today <- utctDay <$> getCurrentTime
    Usage 0 <$> askInt "Код ПЗ" <*> askInt "Код користувача" <*> askDayDefault "Дата" today
            <*> askInt "Тривалість, хв"
  edit _ u = do
    m <- askIntDefault "Тривалість, хв" (usageMinutes u)
    pure u { usageMinutes = m }

instance Editable Review where
  title _ = "Відгук"
  create _ = do
    today <- utctDay <$> getCurrentTime
    Review 0 <$> askInt "Код ПЗ" <*> askInt "Код користувача" <*> askIntRange "Оцінка" 1 5
             <*> ask "Коментар" <*> pure today
  edit _ r = do
    n <- askIntRange "Оцінка" 1 5
    c <- askDefault "Коментар" (reviewComment r)
    pure r { reviewRating = n, reviewComment = c }
