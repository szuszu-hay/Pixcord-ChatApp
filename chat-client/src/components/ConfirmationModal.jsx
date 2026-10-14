import { useEffect } from "react";

function ConfirmationModal({ title, message, confirmLabel = "Confirm", cancelLabel = "Cancel",
    danger = false, isLoading = false, onConfirm, onCancel
}) {
    useEffect(() => {
        const handleKeyDown = (event) => {
            if (event.key === "Escape" && !isLoading) onCancel();
        };
        document.addEventListener("keydown", handleKeyDown);
        return () => document.removeEventListener("keydown", handleKeyDown);
    }, [onCancel, isLoading]);

    const handleOverlayClick = (event) => {
        event.stopPropagation(); 
        if (!isLoading) onCancel();
    };

    return (
        <div className="modal-overlay confirm-overlay" onClick={handleOverlayClick}>
            <div className="modal-content confirm-modal" role="dialog"
                aria-modal="true" aria-labelledby="confirm-title"
                onClick={(e) => e.stopPropagation()}
            >
                <h2 id="confirm-title">{title}</h2>
                <p className="confirm-message">{message}</p>

                <div className="confirm-actions">
                    <button type="button" className="confirm-cancel-btn btn-reset"
                        onClick={onCancel}disabled={isLoading} autoFocus
                    >
                        {cancelLabel}
                    </button>
                    <button type="button" className={`confirm-btn ${danger ? "danger" : ""}`}
                        onClick={onConfirm} disabled={isLoading}
                    >
                        {isLoading ? <span className="spinner" /> : confirmLabel}
                    </button>
                </div>
            </div>
        </div>
    );
}

export default ConfirmationModal;