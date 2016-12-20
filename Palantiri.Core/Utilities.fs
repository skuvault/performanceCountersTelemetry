module Utilities


type CommonHelper = 
    static member SideEffectOnNull act x = if x = null then act(); x else x
    static member SideEffectOnNotNull act x = if x <> null then act(); x else x

