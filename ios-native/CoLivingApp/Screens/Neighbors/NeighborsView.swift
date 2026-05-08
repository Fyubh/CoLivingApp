import SwiftUI

/// Phase 2 placeholder. Phase 6 turns this into a four-section segmented view —
/// Finance / Products / Cleaning / Chat — wired to the real endpoints.
struct NeighborsView: View {
    var body: some View {
        NavigationStack {
            TabPlaceholderBody(
                icon: "person.2",
                hint: "Финансы, покупки, уборка и общий чат соседей."
            )
            .navigationTitle("Соседи")
        }
    }
}

#Preview {
    NeighborsView()
}
