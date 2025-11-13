/**
 * Table Grid Enhancement
 * Adds search, sort, and filter capabilities to HTML tables
 */
(function () {
    'use strict';

    class TableGrid {
        constructor(tableElement, options = {}) {
            this.table = tableElement;
            this.tbody = this.table.querySelector('tbody');
            this.thead = this.table.querySelector('thead');
            this.options = {
                searchable: options.searchable !== false,
                sortable: options.sortable !== false,
                ...options
            };
            this.init();
        }

        init() {
            if (this.options.searchable) {
                this.addSearchBox();
            }
            if (this.options.sortable) {
                this.addSortable();
            }
        }

        addSearchBox() {
            const searchContainer = document.createElement('div');
            searchContainer.className = 'table-grid-search mb-3';
            searchContainer.innerHTML = `
                <div class="row">
                    <div class="col-md-6">
                        <div class="input-group">
                            <span class="input-group-text">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-search" viewBox="0 0 16 16">
                                    <path d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"/>
                                </svg>
                            </span>
                            <input type="text" class="form-control table-grid-search-input" placeholder="Search table...">
                        </div>
                    </div>
                    <div class="col-md-6 text-end">
                        <small class="text-muted table-grid-results-info"></small>
                    </div>
                </div>
            `;

            this.table.parentNode.insertBefore(searchContainer, this.table);

            const searchInput = searchContainer.querySelector('.table-grid-search-input');
            const resultsInfo = searchContainer.querySelector('.table-grid-results-info');

            searchInput.addEventListener('input', (e) => {
                this.filterRows(e.target.value, resultsInfo);
            });

            this.updateResultsInfo(resultsInfo);
        }

        filterRows(searchTerm, resultsInfo) {
            const term = searchTerm.toLowerCase().trim();
            const rows = Array.from(this.tbody.querySelectorAll('tr'));
            let visibleCount = 0;

            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                const isVisible = !term || text.includes(term);
                row.style.display = isVisible ? '' : 'none';
                if (isVisible) visibleCount++;
            });

            this.updateResultsInfo(resultsInfo, visibleCount, rows.length);
        }

        updateResultsInfo(infoElement, visibleCount = null, totalCount = null) {
            if (!infoElement) return;

            if (visibleCount === null) {
                const rows = this.tbody.querySelectorAll('tr');
                totalCount = rows.length;
                visibleCount = totalCount;
            }

            if (visibleCount === totalCount) {
                infoElement.textContent = `Showing ${totalCount} ${totalCount === 1 ? 'entry' : 'entries'}`;
            } else {
                infoElement.textContent = `Showing ${visibleCount} of ${totalCount} ${totalCount === 1 ? 'entry' : 'entries'}`;
            }
        }

        addSortable() {
            const headers = this.thead.querySelectorAll('th');
            headers.forEach((header, index) => {
                // Skip the Actions column (usually the last one)
                if (header.textContent.trim().toLowerCase() === 'actions') {
                    return;
                }

                header.style.cursor = 'pointer';
                header.style.userSelect = 'none';
                header.classList.add('sortable');

                // Add sort indicator
                const sortIcon = document.createElement('span');
                sortIcon.className = 'sort-icon ms-1';
                sortIcon.innerHTML = `
                    <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" fill="currentColor" viewBox="0 0 16 16">
                        <path d="M7.247 11.14 2.451 5.658C1.885 5.013 2.345 4 3.204 4h9.592a1 1 0 0 1 .753 1.659l-4.796 5.48a1 1 0 0 1-1.506 0z"/>
                    </svg>
                `;
                header.appendChild(sortIcon);

                header.addEventListener('click', () => {
                    this.sortTable(index, header);
                });
            });
        }

        sortTable(columnIndex, headerElement) {
            const rows = Array.from(this.tbody.querySelectorAll('tr'));
            const sortIcon = headerElement.querySelector('.sort-icon svg');
            
            // Determine sort direction
            let isAscending = true;
            if (headerElement.dataset.sortDirection === 'asc') {
                isAscending = false;
            }

            // Clear all sort indicators
            this.thead.querySelectorAll('th').forEach(th => {
                th.dataset.sortDirection = '';
                const icon = th.querySelector('.sort-icon svg');
                if (icon) {
                    icon.style.transform = '';
                    icon.style.opacity = '0.3';
                }
            });

            // Set current sort indicator
            headerElement.dataset.sortDirection = isAscending ? 'asc' : 'desc';
            sortIcon.style.transform = isAscending ? 'rotate(180deg)' : '';
            sortIcon.style.opacity = '1';

            // Sort rows
            rows.sort((a, b) => {
                const cellA = a.cells[columnIndex];
                const cellB = b.cells[columnIndex];
                
                if (!cellA || !cellB) return 0;

                let valA = cellA.textContent.trim();
                let valB = cellB.textContent.trim();

                // Try to parse as number
                const numA = parseFloat(valA.replace(/[^0-9.-]/g, ''));
                const numB = parseFloat(valB.replace(/[^0-9.-]/g, ''));

                if (!isNaN(numA) && !isNaN(numB)) {
                    return isAscending ? numA - numB : numB - numA;
                }

                // Try to parse as date
                const dateA = new Date(valA);
                const dateB = new Date(valB);
                if (!isNaN(dateA.getTime()) && !isNaN(dateB.getTime())) {
                    return isAscending ? dateA - dateB : dateB - dateA;
                }

                // String comparison
                return isAscending 
                    ? valA.localeCompare(valB) 
                    : valB.localeCompare(valA);
            });

            // Re-append sorted rows
            rows.forEach(row => this.tbody.appendChild(row));
        }
    }

    // Initialize all tables with class 'table-grid'
    window.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('.table-grid').forEach(table => {
            new TableGrid(table);
        });
    });

    // Export for manual initialization
    window.TableGrid = TableGrid;
})();
