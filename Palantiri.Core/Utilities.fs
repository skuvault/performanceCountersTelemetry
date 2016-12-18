module Utilities


type CommonHelper = 
    static member SideEffectOnNull act x = if x = null then act(); x else x

