import SwiftUI

/// Hospitality residential palette. Brand primary is deep moss (Conifer);
/// accent is rare warm Clay; canvas is warm Linen — never pure white.
enum AppColor {
    static let conifer        = Color(red:  31/255, green:  61/255, blue:  52/255)
    static let coniferPressed = Color(red:  22/255, green:  48/255, blue:  42/255)
    static let linen          = Color(red: 244/255, green: 239/255, blue: 231/255)
    static let clay           = Color(red: 196/255, green: 106/255, blue:  75/255)
    static let ink            = Color(red:  15/255, green:  26/255, blue:  30/255)

    /// Existing call sites bind brand fill via `.accent`; alias keeps them stable.
    static let accent        = conifer
    static let accentPressed = coniferPressed

    static let success       = conifer
    static let warning       = Color(red: 197/255, green: 153/255, blue:  61/255)
    static let danger        = Color(red: 160/255, green:  57/255, blue:  46/255)
    static let dangerPressed = Color(red: 128/255, green:  41/255, blue:  31/255)

    static let inkSecondary  = Color(red: 15/255, green: 26/255, blue: 30/255).opacity(0.55)
    static let inkTertiary   = Color(red: 15/255, green: 26/255, blue: 30/255).opacity(0.32)
    static let hairline      = Color(red: 15/255, green: 26/255, blue: 30/255).opacity(0.10)
    static let glassStroke   = Color.white.opacity(0.18)
}

enum Radius {
    static let cardSm: CGFloat = 20
    static let card:   CGFloat = 26
    static let hero:   CGFloat = 28
    static let sheet:  CGFloat = 32
    static let input:  CGFloat = 16
    static let btn:    CGFloat = 16
    static let chip:   CGFloat = 999
}

/// Modular 6-step rhythm (6/12/18/24/30) carries visual pulse; 4/8/16/20/32
/// remain for compatibility and Apple-system-control padding.
enum Spacing {
    static let s4:  CGFloat = 4
    static let s6:  CGFloat = 6
    static let s8:  CGFloat = 8
    static let s12: CGFloat = 12
    static let s16: CGFloat = 16
    static let s18: CGFloat = 18
    static let s20: CGFloat = 20
    static let s24: CGFloat = 24
    static let s30: CGFloat = 30
    static let s32: CGFloat = 32
}
