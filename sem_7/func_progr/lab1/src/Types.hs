module Types
  ( Category (..)
  , Author (..)
  , Software (..)
  , User (..)
  , Usage (..)
  , Review (..)
  ) where

import Data.Time (Day)
import Database.MySQL.Simple.QueryParams (QueryParams (..))
import Database.MySQL.Simple.QueryResults (QueryResults (..))

data Category = Category
  { categoryId   :: Int
  , categoryName :: String
  } deriving (Show, Eq)

data Author = Author
  { authorId           :: Int
  , authorName         :: String
  , authorOrganization :: String
  , authorEmail        :: Maybe String
  } deriving (Show, Eq)

data Software = Software
  { softwareId         :: Int
  , softwareName       :: String
  , softwareAnnotation :: String
  , softwareCategoryId :: Int
  , softwareVersion    :: String
  , softwarePath       :: String
  , softwareOpenedAt   :: Day
  , softwareValidUntil :: Maybe Day
  , softwareConditions :: String
  } deriving (Show, Eq)

data User = User
  { userId    :: Int
  , userName  :: String
  , userRole  :: String
  , userUnit  :: String
  , userEmail :: Maybe String
  } deriving (Show, Eq)

data Usage = Usage
  { usageId         :: Int
  , usageSoftwareId :: Int
  , usageUserId     :: Int
  , usageDate       :: Day
  , usageMinutes    :: Int
  } deriving (Show, Eq)

data Review = Review
  { reviewId         :: Int
  , reviewSoftwareId :: Int
  , reviewUserId     :: Int
  , reviewRating     :: Int
  , reviewComment    :: String
  , reviewDate       :: Day
  } deriving (Show, Eq)

instance QueryResults Category where
  convertResults fs vs =
    let (i, n) = convertResults fs vs
     in Category i n

instance QueryParams Category where
  renderParams (Category _ n) = renderParams [n]

instance QueryResults Author where
  convertResults fs vs =
    let (i, n, o, e) = convertResults fs vs
     in Author i n o e

instance QueryParams Author where
  renderParams (Author _ n o e) = renderParams (n, o, e)

instance QueryResults Software where
  convertResults fs vs =
    let (i, n, a, c, v, p, o, u, cond) = convertResults fs vs
     in Software i n a c v p o u cond

instance QueryParams Software where
  renderParams s =
    renderParams
      ( softwareName s
      , softwareAnnotation s
      , softwareCategoryId s
      , softwareVersion s
      , softwarePath s
      , softwareOpenedAt s
      , softwareValidUntil s
      , softwareConditions s
      )

instance QueryResults User where
  convertResults fs vs =
    let (i, n, r, u, e) = convertResults fs vs
     in User i n r u e

instance QueryParams User where
  renderParams (User _ n r u e) = renderParams (n, r, u, e)

instance QueryResults Usage where
  convertResults fs vs =
    let (i, s, u, d, m) = convertResults fs vs
     in Usage i s u d m

instance QueryParams Usage where
  renderParams (Usage _ s u d m) = renderParams (s, u, d, m)

instance QueryResults Review where
  convertResults fs vs =
    let (i, s, u, r, c, d) = convertResults fs vs
     in Review i s u r c d

instance QueryParams Review where
  renderParams (Review _ s u r c d) = renderParams (s, u, r, c, d)
