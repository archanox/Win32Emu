#!/usr/bin/env python3
"""
Enrich game database stub with data from Wikidata.

This script fetches metadata from Wikidata using the WikidataKey field
and enriches the stub with missing information.
"""

import sys
import json
import requests
from datetime import datetime
from typing import Optional, Dict, List, Any


class WikidataEnricher:
    """Fetches and enriches game data from Wikidata."""
    
    BASE_URL = "https://www.wikidata.org/w/api.php"
    USER_AGENT = "Win32Emu-GameDB-Bot/1.0 (https://github.com/archanox/Win32Emu)"
    
    # Wikidata property mappings
    PROPERTIES = {
        "P136": "genre",           # genre
        "P178": "developer",       # developer
        "P123": "publisher",       # publisher
        "P577": "publication_date", # publication date
        "P407": "language",        # language of work or name
    }
    
    # ISO 639-1 language code mapping for common Wikidata language entities
    LANGUAGE_CODES = {
        "Q1860": "en",   # English
        "Q150": "fr",    # French
        "Q188": "de",    # German
        "Q1321": "es",   # Spanish
        "Q652": "it",    # Italian
        "Q5146": "pt",   # Portuguese
        "Q7411": "nl",   # Dutch
        "Q9027": "sv",   # Swedish
        "Q9056": "cs",   # Czech
        "Q9058": "pl",   # Polish
        "Q9292": "az",   # Azerbaijani
        "Q8748": "zh",   # Chinese
        "Q5287": "ja",   # Japanese
        "Q9176": "ko",   # Korean
        "Q7737": "ru",   # Russian
    }
    
    def __init__(self):
        self.session = requests.Session()
        self.session.headers.update({"User-Agent": self.USER_AGENT})
        self._entity_cache = {}
    
    def get_entity(self, entity_id: str) -> Optional[Dict[str, Any]]:
        """Fetch entity data from Wikidata."""
        if entity_id in self._entity_cache:
            return self._entity_cache[entity_id]
        
        params = {
            "action": "wbgetentities",
            "ids": entity_id,
            "format": "json",
            "languages": "en"
        }
        
        try:
            response = self.session.get(self.BASE_URL, params=params, timeout=10)
            response.raise_for_status()
            data = response.json()
            entity = data.get("entities", {}).get(entity_id)
            self._entity_cache[entity_id] = entity
            return entity
        except Exception as e:
            print(f"Warning: Failed to fetch entity {entity_id}: {e}", file=sys.stderr)
            return None
    
    def get_label(self, entity_id: str) -> Optional[str]:
        """Get English label for an entity."""
        entity = self.get_entity(entity_id)
        if entity and "labels" in entity and "en" in entity["labels"]:
            return entity["labels"]["en"]["value"]
        return None
    
    def extract_claim_values(self, claims: Dict, property_id: str) -> List[str]:
        """Extract entity IDs from claims."""
        values = []
        if property_id in claims:
            for claim in claims[property_id]:
                if "mainsnak" in claim and "datavalue" in claim["mainsnak"]:
                    datavalue = claim["mainsnak"]["datavalue"]
                    if datavalue.get("type") == "wikibase-entityid":
                        entity_id = datavalue["value"]["id"]
                        values.append(entity_id)
        return values
    
    def parse_date(self, date_value: Dict) -> Optional[str]:
        """Parse Wikidata date format to ISO 8601."""
        try:
            time_str = date_value.get("time", "")
            # Wikidata format: +1997-01-01T00:00:00Z
            # Remove the leading + and parse
            time_str = time_str.lstrip("+")
            precision = date_value.get("precision", 11)
            
            # Parse the date
            dt = datetime.fromisoformat(time_str.replace("Z", "+00:00"))
            
            # Format based on precision
            if precision == 9:  # year
                return dt.strftime("%Y-01-01T00:00:00")
            elif precision == 10:  # month
                return dt.strftime("%Y-%m-01T00:00:00")
            else:  # day or more precise
                return dt.strftime("%Y-%m-%dT%H:%M:%S")
        except Exception as e:
            print(f"Warning: Failed to parse date {date_value}: {e}", file=sys.stderr)
            return None
    
    def enrich_stub(self, stub: Dict[str, Any]) -> Dict[str, Any]:
        """Enrich stub with Wikidata information."""
        wikidata_key = stub.get("WikidataKey")
        if not wikidata_key:
            print("No WikidataKey found in stub, skipping enrichment", file=sys.stderr)
            return stub
        
        print(f"Fetching data from Wikidata for {wikidata_key}...", file=sys.stderr)
        entity = self.get_entity(wikidata_key)
        if not entity:
            print(f"Failed to fetch Wikidata entity {wikidata_key}", file=sys.stderr)
            return stub
        
        claims = entity.get("claims", {})
        enriched = stub.copy()
        
        # Enrich Title if not provided
        if not stub.get("Title"):
            labels = entity.get("labels", {})
            if "en" in labels:
                enriched["Title"] = labels["en"]["value"]
                print(f"  ✓ Title: {enriched['Title']}", file=sys.stderr)
        
        # Enrich Description if not provided
        if not stub.get("Description"):
            descriptions = entity.get("descriptions", {})
            if "en" in descriptions:
                enriched["Description"] = descriptions["en"]["value"]
                print(f"  ✓ Description: {enriched['Description']}", file=sys.stderr)
        
        # Enrich ReleaseDate if not provided
        if not stub.get("ReleaseDate") and "P577" in claims:
            for claim in claims["P577"]:
                if "mainsnak" in claim and "datavalue" in claim["mainsnak"]:
                    date_value = claim["mainsnak"]["datavalue"]["value"]
                    parsed_date = self.parse_date(date_value)
                    if parsed_date:
                        enriched["ReleaseDate"] = parsed_date
                        print(f"  ✓ ReleaseDate: {parsed_date}", file=sys.stderr)
                        break
        
        # Enrich Languages if not provided or empty
        if not stub.get("Languages") or len(stub.get("Languages", [])) == 0:
            language_ids = self.extract_claim_values(claims, "P407")
            languages = []
            for lang_id in language_ids:
                lang_code = self.LANGUAGE_CODES.get(lang_id)
                if lang_code:
                    languages.append(lang_code)
            if languages:
                enriched["Languages"] = languages
                print(f"  ✓ Languages: {', '.join(languages)}", file=sys.stderr)
        
        # Add Wikidata URL to ExternalUrls if not present
        if "ExternalUrls" not in enriched:
            enriched["ExternalUrls"] = {}
        if "Wikidata" not in enriched["ExternalUrls"]:
            enriched["ExternalUrls"]["Wikidata"] = f"https://www.wikidata.org/wiki/{wikidata_key}"
            print(f"  ✓ Added Wikidata URL", file=sys.stderr)
        
        # Note: We don't auto-populate GenreIds, DeveloperIds, PublisherIds
        # because they need to be proper UUIDs, not Wikidata IDs.
        # This would require a mapping table which is a future enhancement.
        # For now, we just log what we found for manual review.
        
        genres = self.extract_claim_values(claims, "P136")
        if genres:
            genre_labels = [self.get_label(g) for g in genres]
            print(f"  ℹ Genres found (not auto-mapped): {', '.join(filter(None, genre_labels))}", file=sys.stderr)
        
        developers = self.extract_claim_values(claims, "P178")
        if developers:
            dev_labels = [self.get_label(d) for d in developers]
            print(f"  ℹ Developers found (not auto-mapped): {', '.join(filter(None, dev_labels))}", file=sys.stderr)
        
        publishers = self.extract_claim_values(claims, "P123")
        if publishers:
            pub_labels = [self.get_label(p) for p in publishers]
            print(f"  ℹ Publishers found (not auto-mapped): {', '.join(filter(None, pub_labels))}", file=sys.stderr)
        
        return enriched


def main():
    """Main entry point."""
    if len(sys.argv) != 3:
        print("Usage: enrich_from_wikidata.py <input.json> <output.json>", file=sys.stderr)
        sys.exit(1)
    
    input_file = sys.argv[1]
    output_file = sys.argv[2]
    
    # Read input stub
    try:
        with open(input_file, 'r', encoding='utf-8') as f:
            stub = json.load(f)
    except Exception as e:
        print(f"Error reading input file: {e}", file=sys.stderr)
        sys.exit(1)
    
    # Enrich with Wikidata
    enricher = WikidataEnricher()
    enriched_stub = enricher.enrich_stub(stub)
    
    # Write output
    try:
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(enriched_stub, f, indent=2, ensure_ascii=False)
        print(f"\nEnriched stub written to {output_file}", file=sys.stderr)
    except Exception as e:
        print(f"Error writing output file: {e}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
