(() => {
    const locationInput = document.getElementById("LocationText");
    const googleMapsUrlInput = document.getElementById("GoogleMapsUrl");
    const cityInput = document.getElementById("City");
    const districtInput = document.getElementById("District");
    const openPickerButton = document.querySelector("[data-map-picker-open]");
    const openMapLink = document.querySelector("[data-map-picker-open-link]");
    const modalElement = document.getElementById("matchPostMapPickerModal");

    if (!locationInput || !googleMapsUrlInput || !cityInput || !districtInput || !openPickerButton || !modalElement) {
        return;
    }

    const searchInput = modalElement.querySelector("[data-map-picker-search]");
    const searchButton = modalElement.querySelector("[data-map-picker-search-button]");
    const currentLocationButton = modalElement.querySelector("[data-map-picker-current-location]");
    const applyButton = modalElement.querySelector("[data-map-picker-apply]");
    const statusBox = modalElement.querySelector("[data-map-picker-status]");
    const mapCanvas = modalElement.querySelector("[data-map-picker-canvas]");
    const addressBox = modalElement.querySelector("[data-map-picker-address]");
    const cityBox = modalElement.querySelector("[data-map-picker-city]");
    const districtBox = modalElement.querySelector("[data-map-picker-district]");
    const coordinatesBox = modalElement.querySelector("[data-map-picker-coordinates]");

    if (!searchInput || !searchButton || !currentLocationButton || !applyButton || !statusBox || !mapCanvas || !addressBox || !cityBox || !districtBox || !coordinatesBox) {
        return;
    }

    const modal = new bootstrap.Modal(modalElement);
    const defaultCenter = [10.7769, 106.7009];
    const defaultZoom = 13;
    const maxTextLength = 300;
    const maxAreaLength = 100;

    let mapInstance = null;
    let marker = null;
    let mapInitialized = false;
    let selectedPlace = null;
    let lookupRequestId = 0;

    const truncate = (value, maxLength) => {
        if (!value) {
            return "";
        }

        const normalized = value.trim();
        return normalized.length > maxLength
            ? normalized.slice(0, maxLength - 3).trimEnd() + "..."
            : normalized;
    };

    const getAddressPart = (address, keys) => {
        for (const key of keys) {
            const value = address?.[key];
            if (typeof value === "string" && value.trim()) {
                return value.trim();
            }
        }

        return "";
    };

    const buildLocationText = (result) => {
        const address = result?.address || {};
        const houseNumber = getAddressPart(address, ["house_number"]);
        const road = getAddressPart(address, ["road", "pedestrian", "footway", "street"]);
        const amenity = getAddressPart(address, ["amenity", "building", "shop", "tourism", "leisure"]);
        const neighbourhood = getAddressPart(address, ["neighbourhood", "quarter", "suburb", "hamlet"]);

        const roadLine = [houseNumber, road].filter(Boolean).join(" ").trim();
        const parts = [amenity, roadLine, neighbourhood].filter(Boolean);

        if (parts.length > 0) {
            return truncate(parts.join(", "), maxTextLength);
        }

        if (typeof result?.display_name === "string" && result.display_name.trim()) {
            return truncate(result.display_name.split(",").slice(0, 3).join(", "), maxTextLength);
        }

        return "";
    };

    const getCityValue = (address) => truncate(
        getAddressPart(address, ["city", "province", "state", "region", "town"]),
        maxAreaLength
    );

    const getDistrictValue = (address) => truncate(
        getAddressPart(address, ["city_district", "district", "county", "suburb", "borough"]),
        maxAreaLength
    );

    const createGoogleMapsUrl = (lat, lng) => {
        return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(`${lat},${lng}`)}`;
    };

    const setStatus = (message, type = "light") => {
        statusBox.className = `match-post-map-status alert alert-${type} border mb-3`;
        statusBox.textContent = message;
    };

    const renderSelectedPlace = () => {
        if (!selectedPlace) {
            addressBox.textContent = "Chua chon vi tri.";
            cityBox.textContent = "-";
            districtBox.textContent = "-";
            coordinatesBox.textContent = "-";
            applyButton.disabled = true;
            return;
        }

        addressBox.textContent = selectedPlace.locationText || "Khong lay duoc dia chi chi tiet.";
        cityBox.textContent = selectedPlace.city || "-";
        districtBox.textContent = selectedPlace.district || "-";
        coordinatesBox.textContent = `${selectedPlace.lat.toFixed(6)}, ${selectedPlace.lng.toFixed(6)}`;
        applyButton.disabled = false;
    };

    const updateOpenMapLink = () => {
        const url = googleMapsUrlInput.value.trim();
        if (!url) {
            openMapLink.classList.add("disabled");
            openMapLink.setAttribute("aria-disabled", "true");
            openMapLink.setAttribute("href", "#");
            return;
        }

        openMapLink.classList.remove("disabled");
        openMapLink.setAttribute("aria-disabled", "false");
        openMapLink.setAttribute("href", url);
    };

    const ensureMap = () => {
        if (mapInitialized) {
            setTimeout(() => {
                mapInstance.invalidateSize();
            }, 150);
            return true;
        }

        if (!window.L) {
            setStatus("Khong tai duoc thu vien ban do. Hay refresh trang va thu lai.", "danger");
            return false;
        }

        mapInstance = window.L.map(mapCanvas, {
            zoomControl: true
        }).setView(defaultCenter, defaultZoom);

        window.L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(mapInstance);

        mapInstance.on("click", (event) => {
            selectCoordinates(event.latlng.lat, event.latlng.lng, {
                shouldCenter: true,
                zoom: Math.max(mapInstance.getZoom(), 16)
            });
        });

        mapInitialized = true;

        setTimeout(() => {
            mapInstance.invalidateSize();
        }, 150);

        return true;
    };

    const placeMarker = (lat, lng, zoom = null) => {
        if (!mapInstance) {
            return;
        }

        if (!marker) {
            marker = window.L.marker([lat, lng]).addTo(mapInstance);
        } else {
            marker.setLatLng([lat, lng]);
        }

        mapInstance.setView([lat, lng], zoom ?? mapInstance.getZoom(), {
            animate: true
        });
    };

    const selectCoordinates = async (lat, lng, options = {}) => {
        const { shouldCenter = false, zoom = 16 } = options;
        const requestId = ++lookupRequestId;

        selectedPlace = {
            lat,
            lng,
            locationText: "",
            city: "",
            district: "",
            googleMapsUrl: createGoogleMapsUrl(lat, lng)
        };
        renderSelectedPlace();

        if (shouldCenter) {
            placeMarker(lat, lng, zoom);
        } else {
            placeMarker(lat, lng);
        }

        setStatus("Dang lay dia chi tu vi tri da chon...", "info");

        try {
            const response = await fetch(
                `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${encodeURIComponent(lat)}&lon=${encodeURIComponent(lng)}&zoom=18&addressdetails=1&accept-language=vi`,
                {
                    headers: {
                        Accept: "application/json"
                    }
                }
            );

            if (!response.ok) {
                throw new Error("Reverse geocoding failed");
            }

            const result = await response.json();
            if (requestId !== lookupRequestId) {
                return;
            }

            const address = result?.address || {};
            selectedPlace = {
                lat,
                lng,
                locationText: buildLocationText(result),
                city: getCityValue(address),
                district: getDistrictValue(address),
                googleMapsUrl: createGoogleMapsUrl(lat, lng)
            };

            renderSelectedPlace();
            setStatus("Da chon vi tri. Bam 'Ap dung vi tri' de dien vao form.", "success");
        } catch (error) {
            if (requestId !== lookupRequestId) {
                return;
            }

            selectedPlace = {
                lat,
                lng,
                locationText: selectedPlace?.locationText || "",
                city: "",
                district: "",
                googleMapsUrl: createGoogleMapsUrl(lat, lng)
            };

            renderSelectedPlace();
            setStatus("Da chon toa do nhung khong lay duoc dia chi chi tiet. Ban van co the ap dung vi tri va sua tay neu can.", "warning");
        }
    };

    const searchPlace = async () => {
        const keyword = searchInput.value.trim();
        if (!keyword) {
            setStatus("Nhap ten dia diem truoc khi tim.", "warning");
            searchInput.focus();
            return;
        }

        setStatus("Dang tim dia diem...", "info");

        try {
            const response = await fetch(
                `https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&addressdetails=1&accept-language=vi&q=${encodeURIComponent(keyword)}`,
                {
                    headers: {
                        Accept: "application/json"
                    }
                }
            );

            if (!response.ok) {
                throw new Error("Search geocoding failed");
            }

            const results = await response.json();
            if (!Array.isArray(results) || results.length === 0) {
                setStatus("Khong tim thay dia diem phu hop. Thu tu khoa cu the hon.", "warning");
                return;
            }

            const firstResult = results[0];
            const lat = Number(firstResult.lat);
            const lng = Number(firstResult.lon);

            if (Number.isNaN(lat) || Number.isNaN(lng)) {
                setStatus("Khong doc duoc toa do tu ket qua tim kiem.", "warning");
                return;
            }

            await selectCoordinates(lat, lng, {
                shouldCenter: true,
                zoom: 16
            });
        } catch (error) {
            setStatus("Khong tim duoc dia diem luc nay. Hay thu lai sau.", "danger");
        }
    };

    const locateCurrentUser = () => {
        if (!navigator.geolocation) {
            setStatus("Trinh duyet nay khong ho tro lay vi tri hien tai.", "warning");
            return;
        }

        setStatus("Dang lay vi tri hien tai cua ban...", "info");

        navigator.geolocation.getCurrentPosition(
            async (position) => {
                await selectCoordinates(position.coords.latitude, position.coords.longitude, {
                    shouldCenter: true,
                    zoom: 17
                });
            },
            () => {
                setStatus("Khong lay duoc vi tri hien tai. Hay kiem tra quyen truy cap vi tri trong trinh duyet.", "warning");
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0
            }
        );
    };

    const tryCenterFromExistingAddress = async () => {
        const existingKeyword = [
            locationInput.value,
            districtInput.value,
            cityInput.value
        ].filter((value) => typeof value === "string" && value.trim()).join(", ");

        if (!existingKeyword) {
            updateOpenMapLink();
            return;
        }

        try {
            const response = await fetch(
                `https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&addressdetails=1&accept-language=vi&q=${encodeURIComponent(existingKeyword)}`,
                {
                    headers: {
                        Accept: "application/json"
                    }
                }
            );

            if (!response.ok) {
                return;
            }

            const results = await response.json();
            if (!Array.isArray(results) || results.length === 0) {
                return;
            }

            const firstResult = results[0];
            const lat = Number(firstResult.lat);
            const lng = Number(firstResult.lon);

            if (Number.isNaN(lat) || Number.isNaN(lng)) {
                return;
            }

            selectedPlace = {
                lat,
                lng,
                locationText: truncate(locationInput.value, maxTextLength),
                city: truncate(cityInput.value, maxAreaLength),
                district: truncate(districtInput.value, maxAreaLength),
                googleMapsUrl: googleMapsUrlInput.value.trim() || createGoogleMapsUrl(lat, lng)
            };

            placeMarker(lat, lng, 16);
            renderSelectedPlace();
        } catch (error) {
            // Best effort only.
        }
    };

    openPickerButton.addEventListener("click", async () => {
        modal.show();

        if (!ensureMap()) {
            return;
        }

        renderSelectedPlace();
        setStatus("Bam vao ban do de chon vi tri, hoac tim theo ten dia diem.", "light");

        if (!selectedPlace) {
            await tryCenterFromExistingAddress();
        } else {
            placeMarker(selectedPlace.lat, selectedPlace.lng);
            renderSelectedPlace();
        }
    });

    modalElement.addEventListener("shown.bs.modal", () => {
        if (mapInstance) {
            mapInstance.invalidateSize();
        }
    });

    searchButton.addEventListener("click", searchPlace);
    searchInput.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            event.preventDefault();
            searchPlace();
        }
    });

    currentLocationButton.addEventListener("click", locateCurrentUser);

    applyButton.addEventListener("click", () => {
        if (!selectedPlace) {
            return;
        }

        if (selectedPlace.locationText) {
            locationInput.value = selectedPlace.locationText;
        }

        cityInput.value = selectedPlace.city || cityInput.value;
        districtInput.value = selectedPlace.district || districtInput.value;
        googleMapsUrlInput.value = selectedPlace.googleMapsUrl;

        updateOpenMapLink();
        modal.hide();
    });

    googleMapsUrlInput.addEventListener("input", updateOpenMapLink);
    updateOpenMapLink();
})();
